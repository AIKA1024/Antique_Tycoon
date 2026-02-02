namespace Antique_Tycoon.Models.Entities;

public class ItemStack<T>(T item, int amount)
{
  public T Item { get; set; } = item;
  public int Amount { get; set; } = amount;
}