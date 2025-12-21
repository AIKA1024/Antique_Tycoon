using System;
using System.Text.Json.Serialization;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

public abstract class RequestBase: ITcpMessage
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}