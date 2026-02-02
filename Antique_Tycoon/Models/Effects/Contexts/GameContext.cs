using Antique_Tycoon.Models.Entities;

namespace Antique_Tycoon.Models.Effects.Contexts;

public abstract class GameContext(Player player)
{
  /// <summary>
  /// 触发效果的玩家（并不一定是效果员工的拥有者）
  /// </summary>
  public Player Player { get; set; } = player;
}