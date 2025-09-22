using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Net;

public class NetClient : NetBase
{
  private readonly UdpClient _udpClient = new();
  private TcpClient? _tcpClient;
  private readonly Dictionary<string, TaskCompletionSource<ITcpMessage>> _pendingRequests = new();
  private readonly Lazy<GameManager> _gameManagerLazy;
  private GameManager GameManagerInstance => _gameManagerLazy.Value;
  public event Action<ITcpMessage>? BroadcastMessageReceived;
  public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(3);

  public NetClient(Lazy<GameManager> gameManagerLazy, string downloadPath)
  {
    _gameManagerLazy = gameManagerLazy;
    DownloadPath = downloadPath;
  }

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
        _ = SendRequestAsync(new HeartbeatMessage { PlayerUuid = GameManagerInstance.LocalPlayer.Uuid }, cancellation);
        await Task.Delay(HeartbeatInterval, cancellation);
      }
      catch (Exception ex)
      {
#if DEBUG
        await App.Current.Services.GetRequiredService<DialogService>().ShowDialogAsync(new MessageDialogViewModel
          { Title = "警告", Message = ex.Message, IsLightDismissEnabled = false });
#endif
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

  #region 封装的发送和接收逻辑

  public async Task<ITcpMessage> SendRequestAsync<T>(T message, CancellationToken cancellationToken = default)
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
        break;
      case TcpMessageType.DownloadMapResponse:
        var downloadMapResponse = JsonSerializer.Deserialize(json, AppJsonContext.Default.DownloadMapResponse);
        message = downloadMapResponse;
        break;
      case TcpMessageType.StartGameResponse:
        var startGameResponse = JsonSerializer.Deserialize(json, AppJsonContext.Default.StartGameResponse);
        message = startGameResponse;
        break;
    }

    if (message == null)
      return Task.CompletedTask;
    if (!_pendingRequests.Remove(message.Id, out var tcs)) //证明不是客户端主动请求的，是服务器主动发送的
    {
      BroadcastMessageReceived?.Invoke(message);
      return Task.CompletedTask;
    }


    tcs.SetResult(message);
    return tcs.Task;
  }

  protected override async Task ReceiveFileChunkAsync(string uuid, string fileName, int chunkIndex, int totalChunks,
    byte[] data)
  {
    await base.ReceiveFileChunkAsync(uuid, fileName, chunkIndex, totalChunks, data);
    if (_pendingRequests.TryGetValue(uuid, out var tcs))
    {
      tcs.SetResult(new DownloadMapResponse { Id = uuid });
      _pendingRequests.Remove(uuid);
    }
  }

  #endregion
}