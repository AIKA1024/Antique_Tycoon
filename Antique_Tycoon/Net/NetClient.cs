using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Net.Udp;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Net;

public class NetClient : NetBase
{
  private readonly UdpClient _udpClient = new();
  private TcpClient? _tcpClient;
  private readonly ConcurrentDictionary<string, TaskCompletionSource<ITcpMessage>> _pendingRequests = new();
  private readonly GameManager _gameManager;
  public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(3);

  public NetClient(GameManager gameManagerLazy, string downloadPath)
  {
    _gameManager = gameManagerLazy;
    DownloadPath = downloadPath;
  }

  public async Task<ServiceInfo> DiscoverRoomAsync()
  {
    _udpClient.EnableBroadcast = true;
    var bytes = "DiscoverRoom"u8.ToArray();
    await _udpClient.SendAsync(bytes, bytes.Length, "255.255.255.255", App.DefaultPort);
    var result = await _udpClient.ReceiveAsync();
    var json = Encoding.UTF8.GetString(result.Buffer);
    var roomInfo = JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.ServiceInfo);
    return roomInfo ?? throw new Exception("Could not deserialize room info");
  }

  private async Task HeartbeatLoopAsync(CancellationToken cancellation = default)
  {
    while (!cancellation.IsCancellationRequested)
    {
      try
      {
        _ = SendRequestAsync(new HeartbeatMessage { PlayerUuid = _gameManager.LocalPlayer.Uuid }, cancellation);
        await Task.Delay(HeartbeatInterval, cancellation);
      }
      catch (Exception ex)
      {
#if DEBUG
        await App.Current.Services.GetRequiredService<DialogService>().ShowDialogAsync(
          new MessageDialogViewModel
            { Title = "警告", Message = ex.Message, IsLightDismissEnabled = false });
#endif
        break;
      }
    }
  }

  public async Task ConnectServer(IPEndPoint ipEndPoint, CancellationToken cancellation = default)
  {
    _tcpClient?.Close();
    _tcpClient = new TcpClient();
    await _tcpClient.ConnectAsync(ipEndPoint, cancellation);
    _ = ReceiveLoopAsync(_tcpClient, cancellation);
    _ = HeartbeatLoopAsync(cancellation); // 开始循环发送心跳包
  }

  #region 封装的发送和接收逻辑

  public async Task<ITcpMessage> SendRequestAsync<T>(T message, CancellationToken cancellationToken = default)
    where T : ITcpMessage
  {
    var data = PackMessage(message);
    var tcs = new TaskCompletionSource<ITcpMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

    _pendingRequests[message.Id] = tcs;
    try
    {
      // 2. 发送数据
      await _tcpClient.GetStream().WriteAsync(data, cancellationToken);

      // 3. 等待结果，显式处理超时或取消
      return await tcs.Task.WaitAsync(cancellationToken);
    }
    catch (OperationCanceledException)
    {
      // 显式让 TCS 失效，防止 HandleReceive 还在尝试设置结果
      tcs.TrySetCanceled(cancellationToken);
      throw;
    }
    catch (Exception ex)
    {
      tcs.TrySetException(ex);
      throw;
    }
    finally
    {
      // 4. 无论如何，必须清理字典，防止内存泄漏
      _pendingRequests.TryRemove(message.Id, out _);
    }
  }

  protected override Task ProcessMessageAsync(TcpMessageType tcpMessageType, string json, TcpClient client)
  {
    var typeInfo = TcpMessageRegistry.Get(tcpMessageType);
    var baseMsg = (ITcpMessage)JsonSerializer.Deserialize(json, typeInfo.jsonTypeInfo);
    TcpMessageRegistry.Dispatch(baseMsg);

    if (baseMsg is IHistoryRecord historyRecord)
    {
      WeakReferenceMessenger.Default.Send(historyRecord);
      Debug.Print("收到IHistoryRecord");
    }

    if (baseMsg == null)
      return Task.CompletedTask;
    if (!_pendingRequests.Remove(baseMsg.Id, out var tcs)) //证明不是客户端主动请求的，是服务器主动发送的
      return Task.CompletedTask;


    tcs.SetResult(baseMsg);
    return tcs.Task;
  }

  protected override async Task ReceiveFileChunkAsync(string uuid, string fileName, int chunkIndex, int totalChunks,
    byte[] data)
  {
    await base.ReceiveFileChunkAsync(uuid, fileName, chunkIndex, totalChunks, data);
    if (_pendingRequests.TryGetValue(uuid, out var tcs))
    {
      tcs.SetResult(new DownloadMapResponse { Id = uuid, FileName = fileName });
      _pendingRequests.Remove(uuid, out _);
    }
  }

  #endregion
}