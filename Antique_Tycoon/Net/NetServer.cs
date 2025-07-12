using System;
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
using Avalonia.Collections;

namespace Antique_Tycoon.Net;

public class NetServer : NetBase
{
  private TcpListener? _listener;
  private readonly string _localIPv4;
  private readonly Room _room = new();

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

  public async Task CreateRoomAndListenAsync(string roomName, CancellationToken cancellation = default)
  {
    var roomInfo = new RoomBaseInfo
    {
      RoomName = roomName,
      Port = App.Current.DefaultPort,
      Ip = _localIPv4,
    };

    _listener?.Dispose();
    _listener = new TcpListener(IPAddress.Any, App.Current.DefaultPort);
    _listener.Start();

    var udpTask = HandleUdpDiscoveryAsync(roomInfo, cancellation);
    var tcpTask = HandleTcpAcceptAsync(cancellation);
    await Task.WhenAll(udpTask, tcpTask);
  }

  /// <summary>
  /// 回应Udp询问房间请求
  /// </summary>
  private async Task HandleUdpDiscoveryAsync(RoomBaseInfo roomInfo, CancellationToken cancellation)
  {
    using var udpServer = new UdpClient(App.Current.DefaultPort);

    while (!cancellation.IsCancellationRequested)
    {
      var result = await udpServer.ReceiveAsync(cancellation);

      var json = JsonSerializer.Serialize(roomInfo);
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
    await ReceiveLoopAsync(stream);
    // 读取数据 / 发送欢迎信息 / 加入房间逻辑
  }

  private async Task ReceiveJoinRoomRequest(JoinRoomRequest request, NetworkStream stream)
  {
    if (_room.Players.Count >= _room.MaxPlayer)
    {
      var response = new JoinRoomResponse
      {
        Id = request.Id,
        Message = "房间已满"
      };
      var data = PackMessage(response);
      await stream.WriteAsync(data, 0, data.Length);
    }
    else
    {
      request.Player.Stream = stream;
      _room.Players.Add(request.Player);
      var response = new JoinRoomResponse
      {
        Id = request.Id,
        Players = _room.Players
      };
      var data = PackMessage(response);
      await stream.WriteAsync(data, 0, data.Length);
    }
  }

  protected override async Task ProcessMessageAsync(TcpMessageType tcpMessageType, string json, NetworkStream stream)
  {
    switch (tcpMessageType)
    {
      case TcpMessageType.JoinRoomRequest:
        if (JsonSerializer.Deserialize(json, AppJsonContext.Default.JoinRoomRequest) is { } joinRoomRequest)
          await ReceiveJoinRoomRequest(joinRoomRequest, stream);
        break;
      case TcpMessageType.ChatMessage:

        break;
    }

    throw new Exception("未知的消息类型");
  }
}