using Avalonia.Collections;

namespace Antique_Tycoon.Models.Net.Tcp;

public class UpdateRoomResponse:ITcpMessage
{
  public required string Id { get; set; }
  public AvaloniaList<Player> Players { get; set; } = [];
}