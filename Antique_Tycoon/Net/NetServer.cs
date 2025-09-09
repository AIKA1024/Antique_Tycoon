using System;
using System.Collections.Concurrent;
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
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Services;
using Microsoft.Extensions.DependencyInjection;
using Timer = System.Timers.Timer;

namespace Antique_Tycoon.Net;

public class NetServer : NetBase//todo 服务器莫名其妙会认为别人掉了
{
  private TcpListener? _listener;
  private readonly string _localIPv4;
  private readonly Room _room = new();
  private readonly ConcurrentDictionary<TcpClient, Player> _clientPlayers = [];
  private readonly Timer _timer = new();

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

  public override event Action<IEnumerable<Player>>? RoomInfoUpdated;
  public Func<Stream>? MapStreamResolver { get; set; }

  public NetServer()
  {
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
    foreach (var kv in _clientPlayers)
    {
      long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
      if (now - kv.Value.LastHeartbeat > DisconnectTimeout.TotalMilliseconds)
      {
        kv.Key.Close();
        _clientPlayers.TryRemove(kv.Key, out _);
        int index = _room.Players.IndexOf(kv.Value); //todo 这个退出房间可以封装一下
        if (index >= 0)
        {
          _room.Players.RemoveAt(index);
          RoomInfoUpdated?.Invoke(_room.Players);
        }
      }
    }
  }

  public async Task CreateRoomAndListenAsync(string roomName, Map map, CancellationToken cancellation = default)
  {
    _clientPlayers.Clear();
    //todo _room好像也要清理一下
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
      // _clientPlayers.Add(client, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
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

  private async Task SendResponseAsync<T>(T message, TcpClient client, CancellationToken cancellationToken = default)
    where T : ITcpMessage
  {
    var data = PackMessage(message);
    await client.GetStream().WriteAsync(data, cancellationToken);
  }

  public void StartGame()
  {
    var startMessage = new StartGameResponse();
    foreach (var tcpClient in _clientPlayers.Keys)
      _ = SendResponseAsync(startMessage, tcpClient, CancellationToken.None);
  }


  private async Task ReceiveJoinRoomRequest(JoinRoomRequest request, TcpClient client)
  {
    if (_room.Players.Count >= _room.MaxPlayer)
    {
      var response = new JoinRoomResponse
      {
        Id = request.Id,
        Message = "房间已满",
        ResponseStatus = RequestResult.Reject
      };
      var data = PackMessage(response);
      await client.GetStream().WriteAsync(data, 0, data.Length);
    }
    else
    {
      _room.Players.Add(request.Player);
      _clientPlayers.TryAdd(client, request.Player);
      var joinRoomResponse = new JoinRoomResponse
      {
        Id = request.Id,
        Players = _room.Players
      };
      await SendResponseAsync(joinRoomResponse, client);

      //todo 给房间的其他人发送更新房间的消息
      var updateRoomResponse = new UpdateRoomResponse
      {
        Id = request.Id,
        Players = _room.Players
      };
      foreach (var otherClient in _clientPlayers.Keys.Where(c => c != client))
        await SendResponseAsync(updateRoomResponse, otherClient);
      RoomInfoUpdated?.Invoke(_room.Players);
    }
  }

  private async Task ReceiveExitRoomRequest(ExitRoomRequest request, TcpClient client)
  {
    _room.Players.Remove(_clientPlayers[client]);
    _clientPlayers.TryRemove(client, out _);
    var updateRoomResponse = new UpdateRoomResponse
    {
      Id = request.Id,
      Players = _room.Players
    };
    foreach (var otherClient in _clientPlayers.Keys.Where(c => c != client)) //发出退出消息的客户端不需要等待服务器回应
      await SendResponseAsync(updateRoomResponse, otherClient);
    RoomInfoUpdated?.Invoke(_room.Players);
  }

  protected override async Task ProcessMessageAsync(TcpMessageType tcpMessageType, string json, TcpClient client)
  {
    switch (tcpMessageType)
    {
      case TcpMessageType.HeartbeatMessage: //心跳包不做处理，因为无论什么请求都会更新最后在线时间
        break;
      case TcpMessageType.JoinRoomRequest:
        if (JsonSerializer.Deserialize(json, AppJsonContext.Default.JoinRoomRequest) is { } joinRoomRequest)
          await ReceiveJoinRoomRequest(joinRoomRequest, client);
        break;
      case TcpMessageType.ExitRoomRequest:
        if (JsonSerializer.Deserialize(json, AppJsonContext.Default.ExitRoomRequest) is { } exitRoomRequest)
          await ReceiveExitRoomRequest(exitRoomRequest, client);
        break;
      case TcpMessageType.DownloadMapRequest:
        if (JsonSerializer.Deserialize(json, AppJsonContext.Default.DownloadMapRequest) is { } downloadMapRequest)
          await SendFileAsync(MapStreamResolver(), downloadMapRequest.Id, "TempMap.zip", TcpMessageType.DownloadMapResponse,client);
        break;
      default:
        throw new Exception("未知的消息类型");
    }

    if (_clientPlayers.TryGetValue(client, out var player))
      player.LastHeartbeat = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
  }
}