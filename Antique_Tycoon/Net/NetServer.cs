using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Net.Tcp.Response.GameAction;
using Antique_Tycoon.Models.Net.Udp;
using Antique_Tycoon.Net.TcpMessageHandlers;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using ServiceInfo = Antique_Tycoon.Models.Net.Udp.ServiceInfo;
using Timer = System.Timers.Timer;

namespace Antique_Tycoon.Net;

public class NetServer : NetBase
{
  private TcpListener? _listener;
  private readonly string _localIPv4;
  private readonly Dictionary<TcpClient, long> _clientLastActiveTimes = [];
  private readonly Timer _timer = new();
  private readonly IEnumerable<ITcpMessageHandler> _messageHandlers;
  private readonly ConcurrentDictionary<string, TaskCompletionSource<ITcpMessage>> _pendingRequests = new();
#if DEBUG
  public TimeSpan DisconnectTimeout { get; set; } = TimeSpan.FromSeconds(9999999999);
#else
  public TimeSpan DisconnectTimeout { get; set; } = TimeSpan.FromSeconds(15);
#endif

  public event Action<TcpClient>? ClientDisConnected;

  public NetServer(IEnumerable<ITcpMessageHandler> messageHandlers, string downloadPath)
  {
    DownloadPath = downloadPath;
    _messageHandlers = messageHandlers;
    _localIPv4 = GetLocalIPv4();
    _timer.Elapsed += (_, _) => CheckOutlineClient();
    _timer.Start();
  }

  private string GetLocalIPv4()
  {
    using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
    // 连接一个远程地址（并不真正发包）
    socket.Connect("8.8.8.8", 65530);
    if (socket.LocalEndPoint is IPEndPoint endPoint)
    {
      return endPoint.Address.ToString(); // 返回本地IP
    }

    return "127.0.0.1"; // 回退
  }

  public TcpListener StartTcpListenerWithAutoRetry(int startPort)
  {
    int currentPort = startPort;
    TcpListener? listener = null;

    while (true) // 循环直到成功绑定
    {
      try
      {
        // 尝试绑定当前端口
        listener = new TcpListener(IPAddress.Any, currentPort);
        listener.Start();
        Console.WriteLine($"成功绑定端口：{currentPort}");
        return listener; // 成功则返回 listener
      }
      catch (SocketException ex)
      {
        // 仅处理“端口已占用”的错误（错误码 10048）
        switch (ex.ErrorCode)
        {
          case 10048:
            Console.WriteLine($"端口 {currentPort} 已被占用，尝试端口 {currentPort + 1}...");
            currentPort++; // 端口+1重试
            listener?.Stop();
            listener = null; // 释放失败的实例
            break;
          case 10013:
            Console.WriteLine($"端口 {currentPort} 可能被系统进程占用，尝试端口 {currentPort + 1}...");
            currentPort++; // 端口+1重试
            listener?.Stop();
            listener = null; // 释放失败的实例
            break;
          default:
            Console.WriteLine($"非端口占用错误：{ex.Message} 停止重试。");
            throw; // 抛出异常让上层处理
        }
      }
      catch (Exception ex)
      {
        // 其他未知错误，终止重试
        Console.WriteLine($"发生错误：{ex.Message}，停止重试。");
        throw;
      }
    }
  }


  private void CheckOutlineClient()
  {
    var disconnectedClients = new List<TcpClient>();

    foreach (var kv in _clientLastActiveTimes)
    {
      long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
      if (now - kv.Value > DisconnectTimeout.TotalMilliseconds)
      {
        disconnectedClients.Add(kv.Key);
      }
    }

    // 统一调用收口方法
    foreach (var client in disconnectedClients)
    {
      Console.WriteLine("玩家心跳超时，准备踢出...");
      OnConnectionLost(client);
    }
  }

  public async Task CreateRoomAndListenAsync(string roomName, Map map, CancellationToken cancellation = default)
  {
    _clientLastActiveTimes.Clear();
    using var ms = new MemoryStream();
    map.Cover.Save(ms);
    var roomInfo = new ServiceInfo
    {
      RoomName = roomName,
      Port = App.DefaultPort,
      Address = _localIPv4,
      CoverData = ms.ToArray(),
      IsLanRoom = true
    };

    _listener?.Dispose();
    _listener = StartTcpListenerWithAutoRetry(App.DefaultPort);
    _listener.Start();

    var udpTask = HandleUdpDiscoveryAsync(roomInfo, cancellation);
    var tcpTask = HandleTcpAcceptAsync(cancellation);
    await Task.WhenAll(udpTask, tcpTask);
  }

  /// <summary>
  /// 回应Udp询问房间请求
  /// </summary>
  private async Task HandleUdpDiscoveryAsync(ServiceInfo roomInfo, CancellationToken cancellationToken)
  {
    using var udpServer = new UdpClient(App.DefaultPort);

    while (!cancellationToken.IsCancellationRequested)
    {
      var result = await udpServer.ReceiveAsync(cancellationToken);

      var json = JsonSerializer.Serialize(roomInfo, Models.Json.AppJsonContext.Default.ServiceInfo);
      var bytes = Encoding.UTF8.GetBytes(json);

      await udpServer.SendAsync(bytes, bytes.Length, result.RemoteEndPoint);
    }
  }

  /// <summary>
  /// 处理Tcp连接请求
  /// </summary>
  private async Task HandleTcpAcceptAsync(CancellationToken cancellation)
  {
    while (!cancellation.IsCancellationRequested)
    {
      var client = await _listener.AcceptTcpClientAsync(cancellation);
      _clientLastActiveTimes.TryAdd(client, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
      _ = ReceiveLoopAsync(client, cancellation); // 处理连接（不要阻塞主循环）
    }
  }

  public async Task SendResponseAsync<T>(T message, TcpClient client, CancellationToken cancellationToken = default)
    where T : ITcpMessage
  {
    Console.WriteLine($"服务器单独发送消息： {typeof(T).Name}");
    var data = PackMessage(message);
    await WriteStreamAsync(client, data, cancellationToken);
  }

  /// <summary>
  /// 接收来自本地玩家的消息（模拟网络接收）
  /// 用于解开 SendRequestAsync 对本地请求的阻塞等待
  /// </summary>
  /// <param name="message">本地产生的响应消息</param>
  public void ReceiveLocalMessage(ITcpMessage message)
  {
    // 1. 检查这个消息是否是对某个挂起请求的响应
    if (!string.IsNullOrEmpty(message.Id) && _pendingRequests.TryRemove(message.Id, out var tcs))
      tcs.TrySetResult(message);
    else
      throw new KeyNotFoundException($"字典中没有消息{message.Id}");
  }

  public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(
    TRequest message,
    TcpClient? client,
    TimeSpan? timeout = null)
    where TRequest : ActionBase
    where TResponse : ITcpMessage
  {
    // 确保消息有 ID (通常在构造函数或发送前生成 GUID)
    if (string.IsNullOrEmpty(message.Id))
      message.Id = Guid.NewGuid().ToString();

    var tcs = new TaskCompletionSource<ITcpMessage>();
    _pendingRequests[message.Id] = tcs;
#if DEBUG
    var effectiveTimeout = timeout ?? Timeout.InfiniteTimeSpan;
#else
    var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
#endif

    try
    {
      // 发送消息
      if (client == null)
      {
        WeakReferenceMessenger.Default.Send(message);
        Console.WriteLine($"服务器本地信使通知 {message.GetType().Name}");
      }
      else
        await SendResponseAsync(message, client);

      // 设置超时处理（可选，防止客户端掉线导致服务器逻辑永久卡死）
      using var cts = new CancellationTokenSource(effectiveTimeout);

      // 等待结果或超时
      await using (cts.Token.Register(() => tcs.TrySetCanceled()))
      {
        var response = await tcs.Task;
        return (TResponse)response;
      }
    }
    catch (OperationCanceledException)
    {
      throw new TimeoutException($"客户端在 {effectiveTimeout.TotalSeconds} 秒内未响应请求: {typeof(TRequest).Name}");
    }
    finally
    {
      // 无论成功失败，移除请求追踪
      _pendingRequests.TryRemove(message.Id, out _);
    }
  }

  public async Task Broadcast<T>(T message, CancellationToken cancellationToken = default) where T : ResponseBase
  {
    var data = PackMessage(message);
    Console.WriteLine($"服务器广播发送消息： {typeof(T).Name}");
    foreach (var client in _clientLastActiveTimes.Keys)
    {
      await WriteStreamAsync(client, data, cancellationToken);
    }
  }


  public async Task BroadcastExcept<T>(T message, TcpClient excluded, CancellationToken cancellationToken = default)
    where T : ResponseBase
  {
    var data = PackMessage(message);
    Console.WriteLine($"服务器广播发送消息： {typeof(T).Name}");
    foreach (var client in _clientLastActiveTimes.Keys)
    {
      if (client == excluded)
        continue;

      await WriteStreamAsync(client, data, cancellationToken);
    }
  }

  protected override async Task ProcessMessageAsync(TcpMessageType tcpMessageType, string json, TcpClient client)
  {
    try
    {
      var jsonType = TcpMessageRegistry.Get(tcpMessageType);
      var baseMsg = (ITcpMessage)JsonSerializer.Deserialize(json, jsonType.jsonTypeInfo); // 服务器发送询问客户端要不要后，客户端还是返回的请求

      if (baseMsg != null && !string.IsNullOrEmpty(baseMsg.Id))
      {
        // 2. 检查是否在挂起列表中
        if (_pendingRequests.TryRemove(baseMsg.Id, out var tcs))
        {
          tcs.SetResult(baseMsg);
          return; // 如果是回复，则不再走下面的普通 Handler 逻辑
        }
      }
    }
    catch (Exception e)
    {
      Console.WriteLine(tcpMessageType.ToString());
      Console.WriteLine(e);
      throw;
    }

    var handlers = _messageHandlers.Where(h => h.CanHandle(tcpMessageType)).ToArray();

    if (handlers.Length != 0)
      foreach (var handler in handlers)
        await handler.HandleAsync(json, client);
    else if (tcpMessageType != TcpMessageType.HeartbeatMessage)
      Console.WriteLine($"未定义{tcpMessageType}的处理方式");

    _clientLastActiveTimes[client] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
  }

  protected override void OnConnectionLost(TcpClient client)
  {
    // 1. 加锁或使用并发字典的安全移除，防止多线程同时触发（比如读写同时报错）
    if (!_clientLastActiveTimes.Remove(client, out _))
    {
      return; // 如果已经被清理过了，直接拦截，防止事件双重触发
    }

    try
    {
      // 2. 安全关闭连接
      if (client.Connected)
      {
        client.Close();
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"关闭客户端时发生异常: {ex.Message}");
    }

    // 3. 触发外部事件，通知业务层（例如让玩家在房间里显示为“掉线”状态）
    ClientDisConnected?.Invoke(client);
    Console.WriteLine("客户端已彻底清理并断开。");
  }
}