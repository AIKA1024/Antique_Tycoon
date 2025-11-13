using Antique_Tycoon.Models;
using Antique_Tycoon.Views.Controls;

namespace Antique_Tycoon.Messages;

public class PlayerMoveMessage(Player player, NodeLinkControl from, NodeLinkControl to)
{
  public Player Player { get; set; } = player;
  public NodeLinkControl From { get; set; } = from;
  public NodeLinkControl To { get; set; } = to;
}