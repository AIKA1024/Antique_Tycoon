using System;
using Antique_Tycoon.Models.Effects.Contexts;
using Antique_Tycoon.Models.Enums;

namespace Antique_Tycoon.Models.Effects.StaffEffectImpls;

public class TaxHikeEffect:IStaffEffect
{
  public decimal Rate { get; set; } = 0.5m;
  public GameTriggerPoint TriggerPoint => GameTriggerPoint.OnCalculateTax;
  public void Execute(GameContext context,Player owner)
  {
    if (context is not PaymentContext paymentContext)
      return;
    
    if (paymentContext.Player == owner)
    {
      // 效果：免费
      paymentContext.Cost = 0;
      paymentContext.Receiver = null; // 也不用给谁钱
      Console.WriteLine("万税爷不需要交税");
    }
    else
    {
      // 效果：钱给拥有者，而不是给银行
      // 只是把收款人改成了主人
      paymentContext.Receiver = owner;
      Console.WriteLine($"税： {paymentContext.Cost} 将支付给 {owner.Name}");
    }
  }

  public string Description => $"其他玩家税收加{Rate * 100}%并全部交给你，免除自己所有税收";
}