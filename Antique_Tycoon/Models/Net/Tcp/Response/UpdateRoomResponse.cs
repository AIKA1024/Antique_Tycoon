using System.Collections.Generic;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class UpdateRoomResponse:ResponseBase
{
  public IEnumerable<Player> Players { get; set; } = [];
}