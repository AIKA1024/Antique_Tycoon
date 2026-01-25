using Antique_Tycoon.Models.Entities;

namespace Antique_Tycoon.Models.Effects.Contexts;

public class EconomyContext(Player player, decimal baseValue) : GameContext(player)
{
  public decimal BaseValue { get; set; } = baseValue;
  public decimal Multiplier { get; set; } = 1.0m;
  public decimal FlatBonus { get; set; } = 0m;
    
  // 如果是卖古玩，可能需要携带物品信息
  // 放在这里或者再派生一个 SellingContext 都可以
  public Antique? TargetItem { get; set; }

  public decimal GetFinalValue() => (BaseValue * Multiplier) + FlatBonus;
}