using System;
using Antique_Tycoon.Models.Effects.Contexts;
using Antique_Tycoon.Models.Enums;

namespace Antique_Tycoon.Models.Effects.StaffEffectImpls;

public class PassStartBonusEffect(decimal bonus):IStaffEffect
{
  public decimal Bonus { get; set; } = bonus;
  public GameTriggerPoint TriggerPoint => GameTriggerPoint.OnPassStartPoint;
  public bool Execute(GameContext context,Player owner)
  {
    if (owner != context.Player)//这个效果只能自己触发
      return false;

    if (context is EconomyContext economyContext)
    {
      economyContext.FlatBonus += Bonus;
      return true;
    }
    return false;
  }

  public string Description => $"路过出生点时额外获得{Bonus}元";
}