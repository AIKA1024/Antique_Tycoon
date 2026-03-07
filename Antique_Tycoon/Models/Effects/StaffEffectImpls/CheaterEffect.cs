using System;
using Antique_Tycoon.Models.Effects.Contexts;
using Antique_Tycoon.Models.Enums;

namespace Antique_Tycoon.Models.Effects.StaffEffectImpls;

public class CheaterEffect : IStaffEffect
{
  public GameTriggerPoint TriggerPoint => GameTriggerPoint.OnAppraisalRoll;

  public bool Execute(GameContext context, Player owner)
  {
    if (owner != context.Player) return false;
    if (context is not DiceContext diceCtx) return false;


    diceCtx.Result += 1;
    Console.WriteLine("老千发动：点数+1");
    return true;
  }

  public string Description => "在获得和掠夺古玩时，骰子点数+1";
}