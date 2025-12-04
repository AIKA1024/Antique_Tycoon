using Antique_Tycoon.Models;
using Antique_Tycoon.Views.Controls;

namespace Antique_Tycoon.Messages;

public class PlayerMoveMessage(string player,  string destinationNodeUuid)
{
  public string PlayerUuid { get; set; } = player;
  public string DestinationNodeUuid { get; set; } = destinationNodeUuid;
}