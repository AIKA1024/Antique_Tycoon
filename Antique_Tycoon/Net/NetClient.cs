using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Models.Net;

namespace Antique_Tycoon.Net;

public class NetClient
{
  private string _localIPv4;
  UdpClient udpClient = new (0);

  public NetClient()
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
  
  public async Task CreateRoomAndListenAsync(string roomName,CancellationToken cancellation = default)
  {
    using var udpServer = new UdpClient(App.Current.DefaultPort);
    var roomInfo = new RoomNetInfo
    {
      RoomName = roomName,
      Port = App.Current.DefaultPort,
      Ip = _localIPv4,
    };
    
    while (true)
    {
      cancellation.ThrowIfCancellationRequested();
      var result = await udpServer.ReceiveAsync(cancellation);
      // var udpClinetInfoJson = Encoding.UTF8.GetString(result.Buffer);
      // var udpClinetInfo = JsonSerializer.Deserialize(udpClinetInfoJson, AppJsonContext.Default.UdpClientInfo);
      
      var json = JsonSerializer.Serialize(roomInfo);
      var bytes = Encoding.UTF8.GetBytes(json);
      await udpServer.SendAsync(bytes, bytes.Length, result.RemoteEndPoint);
    }
  }
  
  
  

  public async Task<RoomNetInfo> DiscoverRoomAsync()
  {
    udpClient.EnableBroadcast = true;
    // var udpClientInfoJson = JsonSerializer.Serialize(udpClientInfo);
    var bytes = Encoding.UTF8.GetBytes("DiscoverRoom");
    
    await udpClient.SendAsync(bytes, bytes.Length, "255.255.255.255", App.Current.DefaultPort);
    var result = await udpClient.ReceiveAsync();
    var json = Encoding.UTF8.GetString(result.Buffer);
    var roomInfo = JsonSerializer.Deserialize(json,AppJsonContext.Default.RoomNetInfo);
    return roomInfo;
  }
}