using System.Collections.Generic;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Response;


/// <summary>
/// 仅用进入房间时给客户端发送玩家信息
/// </summary>
[TcpMessage]
public class UpdateRoomResponse:ResponseBase
{
  public IEnumerable<Player> Players { get; set; } = [];
}