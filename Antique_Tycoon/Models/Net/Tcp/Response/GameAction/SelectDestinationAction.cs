using System.Collections.Generic;

namespace Antique_Tycoon.Models.Net.Tcp.Response.GameAction;

[TcpMessage]
public class SelectDestinationAction(List<string> destinations) : ActionBase
{
  public List<string> Destinations { get; set; } = destinations;
}