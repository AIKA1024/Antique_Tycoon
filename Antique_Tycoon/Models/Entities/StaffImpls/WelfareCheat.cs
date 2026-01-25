using System.Collections.Generic;
using Antique_Tycoon.Models.Effects;
using Antique_Tycoon.Models.Effects.StaffEffectImpls;

namespace Antique_Tycoon.Models.Entities.StaffImpls;

public class WelfareCheat:IStaff
{
  public string Name { get; set; } = "福利欺诈师";
  public List<IStaffEffect> Effects => [new PassStartBonusEffect(500)];
  public decimal HiringCost { get; set; }
  public decimal Salary { get; set; }
}