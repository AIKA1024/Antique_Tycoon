using System;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

public class HeartbeatMessage:RequestBase
{
  public required string PlayerUuid { get; set; }
}