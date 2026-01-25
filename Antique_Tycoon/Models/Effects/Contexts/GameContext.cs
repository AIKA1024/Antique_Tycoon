using Antique_Tycoon.Models.Entities;

namespace Antique_Tycoon.Models.Effects.Contexts;

public abstract class GameContext(Player player)
{
  public Player Player { get; set; } = player;
}