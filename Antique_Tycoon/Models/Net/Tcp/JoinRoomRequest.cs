using Antique_Tycoon.Models.Net.Tcp;

namespace Antique_Tycoon.Models.Net;

public class JoinRoomRequest:ITcpMessage
{
  public required string Id { get; set; }
  
  public required Player Player { get; set; }
}