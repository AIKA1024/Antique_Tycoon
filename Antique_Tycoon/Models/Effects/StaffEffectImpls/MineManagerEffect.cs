using System;
using Antique_Tycoon.Models.Effects.Contexts;
using Antique_Tycoon.Models.Enums;

namespace Antique_Tycoon.Models.Effects.StaffEffectImpls;

public class MineManagerEffect(Player owner) : IStaffEffect
{
  public GameTriggerPoint TriggerPoint => GameTriggerPoint.OnPassMineCharge;

  public void Execute(GameContext context)
  {
    if (context is not PaymentContext paymentContext)
      return;
    
    if (paymentContext.Player == owner)
    {
      // 效果：免费
      paymentContext.Cost = 0;
      paymentContext.Receiver = null; // 也不用给谁钱
      Console.WriteLine("矿主视察：免门票！");
    }
    // 场景 B: 别人路过矿洞
    else
    {
      // 效果：钱给拥有者，而不是给银行
      // 只是把收款人改成了主人
      paymentContext.Receiver = owner;
      Console.WriteLine($"外人进入：门票费 {paymentContext.Cost} 将支付给 {owner.Name}");
    }
  }

  public string Description => "其他玩家下矿时，必须给你下矿费，自己下矿则免费";
}