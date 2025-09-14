using System;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

public class DownloadMapRequest(string hash) : ITcpMessage
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

  public string Hash { get; set; } = hash;
}