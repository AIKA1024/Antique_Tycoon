using System.Collections.Generic;
using Antique_Tycoon.Models.Net.Tcp.Response.GameAction;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class TurnStartResponse:ResponseBase
{
  public required string PlayerUuid { get; set; }
  
  public List<ActionBase> Action { get; set; }
}