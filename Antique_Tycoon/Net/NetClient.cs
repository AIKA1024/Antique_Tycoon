using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Net.Udp;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace Antique_Tycoon.Net;

public class NetClient : NetBase
{
  private readonly UdpClient _udpClient = new();
  private TcpClient? _tcpClient;
  private readonly ConcurrentDictionary<string, TaskCompletionSource<ITcpMessage>> _pendingRequests = new();
  private readonly GameManager _gameManager;
  public event Action? DisconnectedFromServer;
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
        // 使用短暂的超时时间，防止心跳卡死
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellation);
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        _ = SendAsync(new HeartbeatMessage { PlayerUuid = _gameManager.LocalPlayer.Uuid }, cts.Token);
      }
      catch (Exception ex)
      {
        Console.WriteLine($"心跳发送失败: {ex.Message}");
        // 触发断线重连逻辑（见后文）
        // OnDisconnected(); 
      }

      // 无论成功失败，都要等待下一个心跳周期
      await Task.Delay(HeartbeatInterval, cancellation);
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
  public async Task SendAsync<T>(T message, CancellationToken cancellationToken = default) where T : ITcpMessage
  {
    var data = PackMessage(message);
    
    try
    {
      if (_tcpClient is not { Connected: true })
        return;

      // 仅仅写入网络流，不放进 _pendingRequests，也不等待回复
      await _tcpClient.GetStream().WriteAsync(data, cancellationToken);
    }
    catch (IOException)
    {
      // 如果底层流报错，说明真断网了，交给统一出口处理
      Console.WriteLine("发送单向消息失败，网络流已断开");
      OnConnectionLost(_tcpClient); 
    }
    catch (Exception ex)
    {
      Console.WriteLine($"SendAsync 发生异常: {ex.Message}");
    }
  }
  

  public async Task<ITcpMessage?> SendRequestAsync<T>(T message, CancellationToken cancellationToken = default)
    where T : ITcpMessage
  {
    var data = PackMessage(message);
    var tcs = new TaskCompletionSource<ITcpMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

    _pendingRequests[message.Id] = tcs;

    const int maxRetries = 3; // 最大重试次数

    for (int i = 0; i < maxRetries; i++)
    {
      try
      {
        if (_tcpClient is not { Connected: true })
          throw new InvalidOperationException("客户端未连接");

        // 发送数据
        await _tcpClient.GetStream().WriteAsync(data, cancellationToken);

        // 等待结果，增加超时限制防止死等
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(5)); // 5秒收不到回复算超时

        return await tcs.Task.WaitAsync(timeoutCts.Token);
      }
      catch (OperationCanceledException)
      {
        Console.WriteLine($"请求 {message.GetType().Name} 等待超时，尝试重试 {i + 1}/{maxRetries}...");
        if (i == maxRetries - 1)
        {
          tcs.TrySetCanceled(cancellationToken);
          return null; // 超时失败返回 null，业务层根据 null 做UI提示
        }
      }
      catch (IOException) // 捕获网络流异常
      {
        Console.WriteLine("网络波动导致发送失败...");
        await Task.Delay(500, cancellationToken); // 等待半秒后重试
      }
      catch (Exception ex)
      {
        Console.WriteLine($"发送发生未知错误: {ex}");
        break; // 严重错误直接跳出
      }
    }

    _pendingRequests.TryRemove(message.Id, out _);
    return null; // 最终失败
  }

  protected override Task ProcessMessageAsync(TcpMessageType tcpMessageType, string json, TcpClient client)
  {
    var typeInfo = TcpMessageRegistry.Get(tcpMessageType);
    var baseMsg = (ITcpMessage)JsonSerializer.Deserialize(json, typeInfo.jsonTypeInfo);
    TcpMessageRegistry.Dispatch(baseMsg);

    if (baseMsg is IHistoryRecord historyRecord)
    {
      WeakReferenceMessenger.Default.Send(historyRecord);
      Console.WriteLine("收到IHistoryRecord");
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

  #region 掉线处理

  protected override void OnConnectionLost(TcpClient client)
  {
    Console.WriteLine("检测到与服务器的连接已断开...");
    // 1. 关闭底层的 Socket
    try
    {
      _tcpClient?.Close();
    }
    catch (Exception ex)
    {
      Console.WriteLine($"关闭客户端 Socket 时发生异常: {ex.Message}");
    }

    // 2. 【关键！】清理所有还在等待的请求，释放业务层的阻塞
    foreach (var kvp in _pendingRequests)
    {
      kvp.Value.TrySetCanceled(); // 让 SendRequestAsync 里正在 WaitAsync 的任务立即抛出异常结束
    }

    _pendingRequests.Clear();

    // 3. 触发外部事件，通知游戏逻辑层（如 UI 弹窗）
    DisconnectedFromServer?.Invoke();
  }

  #endregion
}