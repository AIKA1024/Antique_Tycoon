namespace Antique_Tycoon.Models.Effects.Contexts;

public class DiceContext(Player player, int initialResult) : GameContext(player)
{
  public int Result { get; set; } = initialResult;

  // 还可以加一些骰子特有的，比如是否允许重投
  public bool CanReroll { get; set; }
}