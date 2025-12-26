using System;

namespace Antique_Tycoon.Models.Net.Tcp;

public abstract class ServiceRequest:ITcpMessage
{
  public string Id { get; set; } = "";
  public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
  
  public string PlayerMoveResponseId { get; set; } = "";
}