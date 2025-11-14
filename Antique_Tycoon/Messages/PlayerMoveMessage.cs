using Antique_Tycoon.Models;
using Antique_Tycoon.Views.Controls;

namespace Antique_Tycoon.Messages;

public class PlayerMoveMessage(string player,  string to)
{
  public string PlayerUuid { get; set; } = player;
  public string NodeToUuid { get; set; } = to;
}