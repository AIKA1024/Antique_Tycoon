namespace Antique_Tycoon.Models.Effects.Contexts;

public class PaymentContext(Player player) : GameContext(player)
{
  public decimal Cost { get; set; }
  public Player? Receiver { get; set; } // null = 银行
}