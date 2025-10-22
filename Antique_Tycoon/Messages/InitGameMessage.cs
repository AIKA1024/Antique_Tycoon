using System.Collections.Generic;
using Antique_Tycoon.Models;

namespace Antique_Tycoon.Messages;

public class InitGameMessage(IEnumerable<Player> players,int currentTurnPlayerIndex)
{
  public IEnumerable<Player> Players { get; set; } = players;
  public int CurrentTurnPlayerIndex { get; set; } = currentTurnPlayerIndex;
}