namespace Antique_Tycoon.Models.Node;

public class AntiqueMapItem//仅用于地图编辑时绑定使用
{
  public Antique AntiqueItem { get; set; }
  public int Amount { get; set; }

  public AntiqueMapItem(Antique antique, int amount)
  {
    AntiqueItem = antique;
    Amount = amount;
  }
}
