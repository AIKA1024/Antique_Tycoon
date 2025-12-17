using System;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

public class ExitRoomRequest : ITcpMessage
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
  public required string PlayerUuid { get; set; }
}