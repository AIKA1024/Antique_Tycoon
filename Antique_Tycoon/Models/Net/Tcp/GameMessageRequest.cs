using System;

namespace Antique_Tycoon.Models.Net.Tcp;

public abstract class GameMessageRequest:ITcpMessage
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
  public string PlayerUuid { get; set; } = "";
}