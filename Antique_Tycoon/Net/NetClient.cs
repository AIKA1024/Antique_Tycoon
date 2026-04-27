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

                await SendAsync(new HeartbeatMessage(_gameManager.LocalPlayer.Uuid), cts.Token).ConfigureAwait(false);
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
        if (_tcpClient is not { Connected: true })
            return;

        // 仅仅写入网络流，不放进 _pendingRequests，也不等待回复
        await WriteStreamAsync(_tcpClient, data, cancellationToken);
    }

    public async Task<ITcpMessage> SendRequestAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : ITcpMessage
    {
        var data = PackMessage(message);
        var tcs = new TaskCompletionSource<ITcpMessage>(TaskCreationOptions.RunContinuationsAsynchronously);

        if (!_pendingRequests.TryAdd(message.Id, tcs))
        {
            throw new InvalidOperationException("消息ID重复");
        }

        // 创建一个安全的超时令牌
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            if (_tcpClient is not { Connected: true })
                throw new InvalidOperationException("客户端未连接");

            await WriteStreamAsync(_tcpClient, data, linkedCts.Token);

            // 使用 cancellationToken 参数来传递超时，而不是依赖回调
            try
            {
                return await tcs.Task.WaitAsync(linkedCts.Token);
            }
            catch (OperationCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                throw new TimeoutException("请求超时，服务端未回复");
            }
        }
        catch (Exception e)
        {
            // 确保只尝试设置一次异常状态
            tcs.TrySetException(e);
            throw;
        }
        finally
        {
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
            Console.WriteLine("收到IHistoryRecord");
        }

        if (baseMsg == null || !_pendingRequests.Remove(baseMsg.Id, out var tcs))
            return Task.CompletedTask;

        tcs.SetResult(baseMsg);
        return tcs.Task;
    }

    protected override void OnFileDownloadCompleted(string uuid, string fileName)
    {
        base.OnFileDownloadCompleted(uuid, fileName);
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

#if DEBUG
    public int PendingRequestsCount => _pendingRequests.Count;
#endif
}