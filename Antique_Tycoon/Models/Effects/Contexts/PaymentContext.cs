namespace Antique_Tycoon.Models.Effects.Contexts;

public class PaymentContext(Player player, decimal cost) : GameContext(player)
{
  public decimal Cost { get; set; } = cost;
  public Player? Receiver { get; set; } // null = 银行
}