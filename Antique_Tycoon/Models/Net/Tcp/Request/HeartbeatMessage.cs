using System;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

[TcpMessage]
public class HeartbeatMessage:RequestBase
{
  public required string PlayerUuid { get; set; }
}