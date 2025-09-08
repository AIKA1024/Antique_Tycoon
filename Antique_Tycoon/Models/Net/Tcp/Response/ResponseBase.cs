using System;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

public abstract class ResponseBase: ITcpMessage
{
  public string Id { get; set; } = "";
  public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
  public RequestResult ResponseStatus;
}