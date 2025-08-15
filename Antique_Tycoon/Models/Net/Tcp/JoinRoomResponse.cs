using System.Collections.ObjectModel;

namespace Antique_Tycoon.Models.Net.Tcp;

public class JoinRoomResponse:ITcpMessage
{
  public string Id { get; set; } = "";
  public ObservableCollection<Player> Players { get; set; } = [];
  
  public string Message { get; set; } = "";
}