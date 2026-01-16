using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class JoinRoomResponse(string roomOwnerUuid) : ResponseBase
{
  // public IEnumerable<Player> Players { get; set; } = []; //用于gameManager只监控广播消息，还是后续发一个updateRoom吧
  
  public string Message { get; set; } = "";
  
  public string RoomOwnerUuid { get; set; } = roomOwnerUuid;
}