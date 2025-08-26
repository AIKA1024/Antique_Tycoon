using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Net;

public class NetClient : NetBase
{
  private readonly UdpClient _udpClient = new();
  private TcpClient? _tcpClient;
  private readonly Dictionary<string, TaskCompletionSource<ITcpMessage>> _pendingRequests = new();
  public override event Action<IEnumerable<Player>>? RoomInfoUpdated;
  public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(3);

  public async Task<RoomBaseInfo> DiscoverRoomAsync()
  {
    _udpClient.EnableBroadcast = true;
    var bytes = "DiscoverRoom"u8.ToArray();
    await _udpClient.SendAsync(bytes, bytes.Length, "255.255.255.255", App.DefaultPort);
    var result = await _udpClient.ReceiveAsync();
    var json = Encoding.UTF8.GetString(result.Buffer);
    var roomInfo = JsonSerializer.Deserialize(json, AppJsonContext.Default.RoomBaseInfo);
    return roomInfo;
  }

  private async Task HeartbeatLoopAsync(CancellationToken cancellation = default)
  {
    while (!cancellation.IsCancellationRequested)
    {
      try
      {
        await SendRequestAsync(new HeartbeatMessage(), cancellation);
        await Task.Delay(HeartbeatInterval, cancellation);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"HeartbeatMessage error: {ex.Message}");
        break;
      }
    }
  }

  public async Task ConnectServer(IPEndPoint ipEndPoint, CancellationToken cancellation = default)
  {
    _tcpClient?.Dispose();
    _tcpClient = new TcpClient();
    await _tcpClient.ConnectAsync(ipEndPoint, cancellation);
    _ = ReceiveLoopAsync(_tcpClient, cancellation); // 开启监听回包任务
    _ = HeartbeatLoopAsync(cancellation); // 开始循环发送心跳包
  }

  public async Task<JoinRoomResponse> JoinRoomAsync(CancellationToken cancellation = default)
  {
    var joinRoomRequest = new JoinRoomRequest
    {
      Player = App.Current.Services.GetRequiredService<Player>()
    };
    return (JoinRoomResponse)await SendRequestAsync(joinRoomRequest, cancellation);
  }

  public void ExitRoom()
  {
    var exitRoomRequest = new ExitRoomRequest();
    _ = SendRequestAsync(exitRoomRequest);
  }

  #region 封装的发送和接收逻辑

  private async Task<ITcpMessage> SendRequestAsync<T>(T message, CancellationToken cancellationToken = default)
    where T : ITcpMessage
  {
    var data = PackMessage(message);
    var tcs = new TaskCompletionSource<ITcpMessage>();
    _pendingRequests[message.Id] = tcs;
    await _tcpClient.GetStream().WriteAsync(data, cancellationToken);
    // 等待服务器响应（HandleReceive 中会完成 tcs）
    return await tcs.Task.WaitAsync(cancellationToken);
  }


  protected override Task ProcessMessageAsync(TcpMessageType tcpMessageType, string json, TcpClient client)
  {
    ITcpMessage? message = null; //额外处理，调用方只需await方法就行，不需要在这里添加逻辑
    switch (tcpMessageType)
    {
      case TcpMessageType.JoinRoomResponse:
        var joinRoomResponse = JsonSerializer.Deserialize(json, AppJsonContext.Default.JoinRoomResponse);
        message = joinRoomResponse;
        break;
      case TcpMessageType.UpdateRoomResponse:
        var updateRoomResponse = JsonSerializer.Deserialize(json, AppJsonContext.Default.UpdateRoomResponse);
        message = updateRoomResponse;
        RoomInfoUpdated?.Invoke(updateRoomResponse.Players);
        break;
    }


    if (_pendingRequests.TryGetValue(message.Id, out var tcs))
    {
      tcs.SetResult(message);
      _pendingRequests.Remove(message.Id);
      return tcs.Task;
    }

    return Task.CompletedTask;
  }

  #endregion
}