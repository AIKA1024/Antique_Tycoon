using System.Collections.ObjectModel;

namespace Antique_Tycoon.Models.Net.Tcp;

public class UpdateRoomResponse:ITcpMessage
{
  public required string Id { get; set; }
  public ObservableCollection<Player> Players { get; set; } = [];
}