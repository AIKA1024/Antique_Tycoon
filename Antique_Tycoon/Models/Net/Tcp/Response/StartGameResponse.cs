using System;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class StartGameResponse:ITcpMessage
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}