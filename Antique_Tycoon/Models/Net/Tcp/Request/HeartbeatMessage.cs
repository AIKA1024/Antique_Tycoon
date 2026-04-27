using System;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

[TcpMessage]
public class HeartbeatMessage(string playerUuid):RequestBase
{
  public string PlayerUuid { get; set; } = playerUuid;
}