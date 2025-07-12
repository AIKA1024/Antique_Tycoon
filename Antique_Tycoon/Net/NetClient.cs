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
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Net;
//todo aot后好像无法连接
public class NetClient : NetBase
{
  private readonly UdpClient _udpClient = new();
  private TcpClient? _tpcClient;
  private readonly Dictionary<string, TaskCompletionSource<ITcpMessage>> _pendingRequests = new();
  private NetworkStream? _stream;

  public async Task<RoomBaseInfo> DiscoverRoomAsync()
  {
    _udpClient.EnableBroadcast = true;
    var bytes = "DiscoverRoom"u8.ToArray();
    await _udpClient.SendAsync(bytes, bytes.Length, "255.255.255.255", App.Current.DefaultPort);
    var result = await _udpClient.ReceiveAsync();
    var json = Encoding.UTF8.GetString(result.Buffer);
    var roomInfo = JsonSerializer.Deserialize(json, AppJsonContext.Default.RoomBaseInfo);
    return roomInfo;
  }

  private async Task ConnectServer(IPEndPoint ipEndPoint, CancellationToken cancellation = default)
  {
    _tpcClient?.Dispose();
    _tpcClient = new TcpClient();
    await _tpcClient.ConnectAsync(ipEndPoint, cancellation);
    _stream = _tpcClient.GetStream();
    _ = ReceiveLoopAsync(_stream, cancellation); // ✅ 开启监听回包任务
  }

  public async Task<JoinRoomResponse> JoinRoomAsync(IPEndPoint ipEndPoint, CancellationToken cancellation = default)
  {
    await ConnectServer(ipEndPoint, cancellation);
    var joinRoomRequest = new JoinRoomRequest
    {
      Id = Guid.CreateVersion7().ToString(),
      Player = App.Current.Services.GetRequiredService<Player>()
    };
    return (JoinRoomResponse)await SendRequestAsync(joinRoomRequest, cancellation);
  }

  private async Task<ITcpMessage> SendRequestAsync<T>(T message, CancellationToken cancellationToken = default) where T : ITcpMessage
  {
    var data = PackMessage(message);
    var tcs = new TaskCompletionSource<ITcpMessage>();
    _pendingRequests[message.Id] = tcs;
    await _stream.WriteAsync(data, cancellationToken);
    // 等待服务器响应（HandleReceive 中会完成 tcs）
    return await tcs.Task.WaitAsync(cancellationToken);
  }

  protected override Task ProcessMessageAsync(TcpMessageType tcpMessageType, string json, NetworkStream stream)
  {
    ITcpMessage? message = null;
    switch (tcpMessageType)
    {
      case TcpMessageType.JoinRoomResponse:
        message = JsonSerializer.Deserialize(json, AppJsonContext.Default.JoinRoomResponse);
        break;
      case TcpMessageType.ChatMessage:

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
}