using System;
using Antique_Tycoon.Models.Effects.Contexts;
using Antique_Tycoon.Models.Enums;

namespace Antique_Tycoon.Models.Effects.StaffEffectImpls;

public class PassStartBonusEffect(decimal bonus):IStaffEffect
{
  public decimal Bonus { get; set; } = bonus;
  public GameTriggerPoint TriggerPoint => GameTriggerPoint.OnPassStartPoint;
  public void Execute(GameContext context)
  {
    if (context is EconomyContext economyContext)
      economyContext.FlatBonus += Bonus;
  }

  public string Description => $"路过出生点时额外获得{Bonus}元";
}