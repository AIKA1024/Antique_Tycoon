using System.Collections.Generic;
using Antique_Tycoon.Models.Effects;
using Antique_Tycoon.Models.Effects.StaffEffectImpls;

namespace Antique_Tycoon.Models.Entities.StaffImpls;

public class TaxEvasionKing:IStaff
{
  public string Name { get; set; } = "漏税王";
  public List<IStaffEffect> Effects { get; set; } = [new TaxEvasionEffect()];
  public decimal HiringCost { get; set; } = 1000;
  public decimal Salary { get; set; }
}