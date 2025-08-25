using System;
using System.Collections.ObjectModel;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class UpdateRoomResponse:ITcpMessage
{
  public required string Id { get; set; }
  public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
  public ObservableCollection<Player> Players { get; set; } = [];
}