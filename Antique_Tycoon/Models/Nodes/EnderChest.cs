namespace Antique_Tycoon.Models.Nodes;

public class EnderChest : NodeModel
{
  public override string? Tooltip
  {
    get => !string.IsNullOrEmpty(field) ? field : "可以顺走其他玩家的古玩";
    set;
  }
}