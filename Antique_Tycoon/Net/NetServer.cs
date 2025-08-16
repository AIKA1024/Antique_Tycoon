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

namespace Antique_Tycoon.Net;

public class NetServer : NetBase
{
  private TcpListener? _listener;
  private readonly string _localIPv4;
  private readonly Room _room = new();
  public override event Action<IEnumerable<Player>>? RoomInfoUpdated;
  

  public NetServer()
  {
    _localIPv4 = GetLocalIPv4();
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

  public async Task CreateRoomAndListenAsync(string roomName,Map map, CancellationToken cancellation = default)
  {
    using var ms = new MemoryStream();
    map.Cover.Save(ms);
    var roomInfo = new RoomBaseInfo
    {
      RoomName = roomName,
      Port = App.DefaultPort,
      Ip = _localIPv4,
      CoverData = ms.ToArray(),
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

      var json = JsonSerializer.Serialize(roomInfo,AppJsonContext.Default.RoomBaseInfo);
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
      _ = HandleTcpClientAsync(client); // 处理连接（不要阻塞主循环）
    }
  }

  /// <summary>
  /// 处理Tcp消息
  /// </summary>
  private async Task HandleTcpClientAsync(TcpClient client)
  {
    await using var stream = client.GetStream();
    await ReceiveLoopAsync(client);
  }

  private async Task SendRequestAsync<T>(T message, TcpClient client, CancellationToken cancellationToken = default)
    where T : ITcpMessage
  {
    var data = PackMessage(message);
    await client.GetStream().WriteAsync(data, cancellationToken);
  }

  private async Task ReceiveJoinRoomRequest(JoinRoomRequest request, TcpClient client)
  {
    if (_room.Players.Count >= _room.MaxPlayer)
    {
      var response = new JoinRoomResponse
      {
        Id = request.Id,
        Message = "房间已满"
      };
      var data = PackMessage(response);
      await client.GetStream().WriteAsync(data, 0, data.Length);
    }
    else
    {
      request.Player.Client = client;
      _room.Players.Add(request.Player);
      var joinRoomResponse = new JoinRoomResponse
      {
        Id = request.Id,
        Players = _room.Players
      };
      await SendRequestAsync(joinRoomResponse, client);

      //todo 给房间的其他人发送更新房间的消息
      var updateRoomResponse = new UpdateRoomResponse
      {
        Id = request.Id,
        Players = _room.Players
      };
      foreach (var player in _room.Players.Where(p => !p.IsHomeowner && p.Client != client))
        await SendRequestAsync(updateRoomResponse, player.Client);
      RoomInfoUpdated?.Invoke(_room.Players);
    }
  }


  protected override async Task ProcessMessageAsync(TcpMessageType tcpMessageType, string json, TcpClient client)
  {
    switch (tcpMessageType)
    {
      case TcpMessageType.JoinRoomRequest:
        if (JsonSerializer.Deserialize(json, AppJsonContext.Default.JoinRoomRequest) is { } joinRoomRequest)
          await ReceiveJoinRoomRequest(joinRoomRequest, client);
        break;
    }

    throw new Exception("未知的消息类型");
  }
}