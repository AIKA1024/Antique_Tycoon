namespace Antique_Tycoon.Models.Entities;

public class AntiqueStack
{
  public Antique Item { get; set; }
  public int Amount { get; set; }

  public AntiqueStack(Antique antique, int amount)
  {
    Item = antique;
    Amount = amount;
  }
}
