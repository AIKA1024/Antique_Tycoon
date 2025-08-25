using System;
using System.Collections.ObjectModel;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class JoinRoomResponse:ITcpMessage
{
  public string Id { get; set; } = "";//因为是回应，所以不需要初始化一个真正uuid,使用客户端发送过来的就行
  public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
  public ObservableCollection<Player> Players { get; set; } = [];
  
  public string Message { get; set; } = "";
}