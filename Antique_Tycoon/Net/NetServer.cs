using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Models.Net;

namespace Antique_Tycoon.Net;

public class NetServer
{
  private TcpListener? _listener;
  private readonly string _localIPv4;

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
    var roomInfo = new RoomNetInfo
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
  private async Task HandleUdpDiscoveryAsync(RoomNetInfo roomInfo, CancellationToken cancellation)
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
  private async Task HandleTcpClientAsync(TcpClient client)
  {
    using var stream = client.GetStream();
    // 读取数据 / 发送欢迎信息 / 加入房间逻辑
  }
}