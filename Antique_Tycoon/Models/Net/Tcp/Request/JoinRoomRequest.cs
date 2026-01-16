using System;
namespace Antique_Tycoon.Models.Net.Tcp.Request;

[TcpMessage]
public class JoinRoomRequest : RequestBase
{
  public required Player Player { get; set; }
  public string PlayerUuid => Player.Uuid;
}