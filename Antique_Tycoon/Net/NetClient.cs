using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Antique_Tycoon.Net;

public class NetClient
{
  public async Task CreateRoom(string roomName)
  {
    var udp = new UdpClient(13437);
    while (true)
    {
      var result = await udp.ReceiveAsync();
      string msg = Encoding.UTF8.GetString(result.Buffer);

      if (msg == "DISCOVER_ROOM")
      {
        var response = Encoding.UTF8.GetBytes($"ROOM:{roomName}@192.168.1.100");
        await udp.SendAsync(response, response.Length, result.RemoteEndPoint);
      }
    }
  }

  public async Task DiscoverRoom()
  {
    UdpClient udp = new UdpClient();
    udp.EnableBroadcast = true;

    var message = "DISCOVER_ROOM"u8.ToArray();
    await udp.SendAsync(message, message.Length, new IPEndPoint(IPAddress.Broadcast, 13437));
    var result = await udp.ReceiveAsync();
    string msg = Encoding.UTF8.GetString(result.Buffer);
  }
}