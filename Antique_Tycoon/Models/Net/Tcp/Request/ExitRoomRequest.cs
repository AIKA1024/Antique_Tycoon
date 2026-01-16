using System;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

[TcpMessage]
public class ExitRoomRequest : RequestBase
{
  public required string PlayerUuid { get; set; }
}