namespace Antique_Tycoon.Models.Nodes;

public class Ender:NodeModel
{
  public override string? Tooltip
  {
    get => !string.IsNullOrEmpty(field) ? field : "随机失去一个伙计";
    set;
  }
}