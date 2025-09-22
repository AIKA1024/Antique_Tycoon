using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class UpdateRoomResponse:ResponseBase
{
  public IEnumerable<Player> Players { get; set; } = [];
}