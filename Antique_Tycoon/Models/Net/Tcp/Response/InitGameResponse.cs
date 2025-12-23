using System.Collections.Generic;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class InitGameResponse(IEnumerable<Player> players,int currentTurnPlayerIndex):ResponseBase
{
  public IEnumerable<Player> Players { get; set; } = players;
  public int CurrentTurnPlayerIndex { get; set; } = currentTurnPlayerIndex;
}