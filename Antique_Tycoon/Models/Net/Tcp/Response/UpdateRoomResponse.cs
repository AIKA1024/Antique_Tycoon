using System;
using System.Collections.ObjectModel;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class UpdateRoomResponse:ResponseBase
{
  public ObservableCollection<Player> Players { get; set; } = [];
}