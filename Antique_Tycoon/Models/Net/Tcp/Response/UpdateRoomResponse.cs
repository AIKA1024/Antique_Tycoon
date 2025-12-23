using System.Collections.Generic;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

/// <summary>
/// 仅用进入房间时给客户端发送玩家信息
/// </summary>
public class UpdateRoomResponse:ResponseBase
{
  public IEnumerable<Player> Players { get; set; } = [];
}