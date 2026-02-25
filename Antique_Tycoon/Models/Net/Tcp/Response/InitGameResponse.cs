using System.Collections.Generic;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class InitGameResponse(IEnumerable<Player> players,int currentTurnPlayerIndex):ResponseBase
{
  public IEnumerable<Player> Players { get; set; } = players;
  
  public int CurrentTurnPlayerIndex { get; set; } = currentTurnPlayerIndex;
}