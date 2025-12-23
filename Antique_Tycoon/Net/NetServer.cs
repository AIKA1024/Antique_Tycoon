using System;
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
using Antique_Tycoon.Net.TcpMessageHandlers;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Timer = System.Timers.Timer;

namespace Antique_Tycoon.Net;

public class NetServer : NetBase
{
  private TcpListener? _listener;
  private readonly string _localIPv4;
  private readonly Dictionary<TcpClient, long> _clientLastActiveTimes = [];
  private readonly Timer _timer = new();
  private readonly IEnumerable<ITcpMessageHandler> _messageHandlers;
  private readonly System.Collections.Concurrent.ConcurrentDictionary<string, TaskCompletionSource<ITcpMessage>> _pendingRequests = new();
#if Debug
  public TimeSpan DisconnectTimeout { get; set; } = TimeSpan.FromSeconds(9999999999);
#else
  public TimeSpan DisconnectTimeout { get; set; } = TimeSpan.FromSeconds(15);
#endif
  
  public event Action<TcpClient> ClientDisConnected;
  
  /// <summary>
  /// 心跳包间隔
  /// </summary>
  public TimeSpan CheckOutlineInterval
  {
    get;
    set
    {
      field = value;
      _timer.Interval = field.TotalMilliseconds;
    }
  } = TimeSpan.FromSeconds(5);

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
        if (ex.ErrorCode == 10048)
        {
          Console.WriteLine($"端口 {currentPort} 已被占用，尝试端口 {currentPort + 1}...");
          currentPort++; // 端口+1重试
          listener?.Stop();
          listener = null; // 释放失败的实例
        }
        else
        {
          // 其他网络错误（如权限不足、端口超出范围等），终止重试
          Console.WriteLine($"非端口占用错误：{ex.Message}，停止重试。");
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
    foreach (var kv in _clientLastActiveTimes)
    {
      long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
      if (!(now - kv.Value > DisconnectTimeout.TotalMilliseconds)) continue;
      kv.Key.Close();
      _clientLastActiveTimes.Remove(kv.Key);
      ClientDisConnected?.Invoke(kv.Key);
    }
  }

  public async Task CreateRoomAndListenAsync(string roomName, Map map, CancellationToken cancellation = default)
  {
    _clientLastActiveTimes.Clear();
    using var ms = new MemoryStream();
    map.Cover.Save(ms);
    var roomInfo = new RoomBaseInfo
    {
      RoomName = roomName,
      Port = App.DefaultPort,
      Ip = _localIPv4,
      CoverData = ms.ToArray(),
      Hash = App.Current.Services.GetRequiredService<MapFileService>().GetMapFileHash(map),
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
  private async Task HandleUdpDiscoveryAsync(RoomBaseInfo roomInfo, CancellationToken cancellationToken)
  {
    using var udpServer = new UdpClient(App.DefaultPort);

    while (!cancellationToken.IsCancellationRequested)
    {
      var result = await udpServer.ReceiveAsync(cancellationToken);

      var json = JsonSerializer.Serialize(roomInfo, AppJsonContext.Default.RoomBaseInfo);
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
      // _ = HandleTcpClientAsync(client); // 处理连接（不要阻塞主循环）
      await ReceiveLoopAsync(client);
    }
  }

  /// <summary>
  /// 处理Tcp消息
  /// </summary>
  // private async Task HandleTcpClientAsync(TcpClient client)
  // {
  //   // await using var stream = client.GetStream();
  //   await ReceiveLoopAsync(client);
  // }

  public async Task SendResponseAsync<T>(T message, TcpClient client, CancellationToken cancellationToken = default)
    where T : ITcpMessage
  {
    var data = PackMessage(message);
    await client.GetStream().WriteAsync(data, cancellationToken);
  }
  
  public async Task<TResponse> SendRequestAsync<TRequest, TResponse>(
    TRequest message, 
    TcpClient client, 
    TimeSpan? timeout = null) 
    where TRequest : ServiceRequest 
    where TResponse : ITcpMessage
  {
    // 确保消息有 ID (通常在构造函数或发送前生成 GUID)
    if (string.IsNullOrEmpty(message.Id))
    {
      message.Id = Guid.NewGuid().ToString();
    }

    var tcs = new TaskCompletionSource<ITcpMessage>();
    _pendingRequests[message.Id] = tcs;
    
    var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
    try
    {
      // 发送消息
      await SendResponseAsync(message, client);//只计算
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

  public async Task Broadcast<T>(T message, CancellationToken cancellationToken = default)where T : ResponseBase
  {
    var data = PackMessage(message);
    foreach (var client in _clientLastActiveTimes.Keys)
      await client.GetStream().WriteAsync(data, cancellationToken);
  }
  
  public async Task BroadcastExcept<T>(T message,TcpClient excluded, CancellationToken cancellationToken = default)where T : ResponseBase
  {
    var data = PackMessage(message);
    foreach (var client in _clientLastActiveTimes.Keys)
    {
      if (client == excluded)
        continue;

      await client.GetStream().WriteAsync(data, cancellationToken);
    }
  }

  protected override async Task ProcessMessageAsync(TcpMessageType tcpMessageType, string json, TcpClient client)
  {
    try
    {
      var baseMsg = (ITcpMessage)JsonSerializer.Deserialize(json, GetJsonTypeInfo(tcpMessageType)); // 服务器发送询问客户端要不要后，客户端还是返回的请求

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
      Debug.WriteLine($"未定义{tcpMessageType}的处理方式");

    _clientLastActiveTimes[client] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
  }
}