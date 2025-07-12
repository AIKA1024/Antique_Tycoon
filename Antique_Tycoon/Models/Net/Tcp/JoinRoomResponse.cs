using Avalonia.Collections;

namespace Antique_Tycoon.Models.Net.Tcp;

public class JoinRoomResponse:ITcpMessage
{
  public string Id { get; set; } = "";
  public AvaloniaList<Player> Players { get; set; } = [];
  
  public string Message { get; set; } = "";
}