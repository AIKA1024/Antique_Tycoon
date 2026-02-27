using Antique_Tycoon.Models.Effects.Contexts;
using Antique_Tycoon.Models.Enums;

namespace Antique_Tycoon.Models.Effects.StaffEffectImpls;

public class TaxEvasionEffect : IStaffEffect
{
  public GameTriggerPoint TriggerPoint => GameTriggerPoint.OnCalculateTax;

  public bool Execute(GameContext context, Player owner)
  {
    if (context is EconomyContext economyContext)
    {
      economyContext.Multiplier = 0.5m;
      return true;
    }

    return false;
  }

  public string Description => "每回合的税少交50%";
}