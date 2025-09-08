using System;
using System.Collections.ObjectModel;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class JoinRoomResponse:ResponseBase
{
  public ObservableCollection<Player> Players { get; set; } = [];
  
  public string Message { get; set; } = "";
}