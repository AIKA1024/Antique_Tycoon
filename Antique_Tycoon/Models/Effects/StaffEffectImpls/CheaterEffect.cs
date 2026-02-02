using System;
using Antique_Tycoon.Models.Effects.Contexts;
using Antique_Tycoon.Models.Enums;

namespace Antique_Tycoon.Models.Effects.StaffEffectImpls;

public class CheaterEffect : IStaffEffect
{
  public GameTriggerPoint TriggerPoint => GameTriggerPoint.OnAppraisalRoll;
  public void Execute(GameContext context,Player owner)
  {
    if (context is DiceContext diceCtx) 
    {
      diceCtx.Result += 1;
      Console.WriteLine("老千发动：点数+1");
    }
  }

  public string Description => "在获得和掠夺古玩时，骰子点数+1";
}