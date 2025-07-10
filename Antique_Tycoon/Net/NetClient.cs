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
  private UdpClient udpClient = new();
  private TcpClient? _tpcClient;

  public async Task<RoomNetInfo> DiscoverRoomAsync()
  {
    udpClient.EnableBroadcast = true;
    var bytes = "DiscoverRoom"u8.ToArray();
    await udpClient.SendAsync(bytes, bytes.Length, "255.255.255.255", App.Current.DefaultPort);
    var result = await udpClient.ReceiveAsync();
    var json = Encoding.UTF8.GetString(result.Buffer);
    var roomInfo = JsonSerializer.Deserialize(json, AppJsonContext.Default.RoomNetInfo);
    return roomInfo;
  }

  public async Task JoinRoomAsync(IPEndPoint ipEndPoint, CancellationToken cancellation = default)
  {
    _tpcClient?.Dispose();
    _tpcClient = new TcpClient();
    await _tpcClient.ConnectAsync(ipEndPoint,cancellation);
  }
}