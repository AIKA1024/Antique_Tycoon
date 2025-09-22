using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Net.TcpMessageHandlers;
using Antique_Tycoon.Services;
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
  /// <summary>
  /// 服务器才会收到这个事件
  /// </summary>
  public event Func<TcpClient,Task>? ClientDisconnected;//应该提供一个非异步的事件，但我自己用就这样吧
  public TimeSpan DisconnectTimeout { get; set; } = TimeSpan.FromSeconds(10);

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
    _timer.Elapsed += (_, _) => CheckOutlinePlayer();
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

  private void CheckOutlinePlayer()
  {
    foreach (var kv in _clientLastActiveTimes)
    {
      long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
      if (now - kv.Value > DisconnectTimeout.TotalMilliseconds)
      {
        kv.Key.Close();
        _clientLastActiveTimes.Remove(kv.Key);
        ClientDisconnected?.Invoke(kv.Key);
      }
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
    _listener = new TcpListener(IPAddress.Any, App.DefaultPort);
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
      _ = HandleTcpClientAsync(client); // 处理连接（不要阻塞主循环）
    }
  }

  /// <summary>
  /// 处理Tcp消息
  /// </summary>
  private async Task HandleTcpClientAsync(TcpClient client)
  {
    // await using var stream = client.GetStream();
    await ReceiveLoopAsync(client);
  }

  public async Task SendResponseAsync<T>(T message, TcpClient client, CancellationToken cancellationToken = default)
    where T : ITcpMessage
  {
    var data = PackMessage(message);
    await client.GetStream().WriteAsync(data, cancellationToken);
  }

  public async Task Broadcast<T>(T message, CancellationToken cancellationToken = default)where T : ITcpMessage
  {
    var data = PackMessage(message);
    foreach (var client in _clientLastActiveTimes.Keys)
      await client.GetStream().WriteAsync(data, cancellationToken);
  }
  
  public async Task BroadcastExcept<T>(T message,TcpClient excluded, CancellationToken cancellationToken = default)where T : ITcpMessage
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
    var handlers = _messageHandlers.Where(h => h.CanHandle(tcpMessageType)).ToArray();

    if (handlers.Length != 0)
      foreach (var handler in handlers)
        await handler.HandleAsync(json, client);
    else
      Console.WriteLine($"未定义{tcpMessageType}的处理方式");

    _clientLastActiveTimes[client] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
  }
}