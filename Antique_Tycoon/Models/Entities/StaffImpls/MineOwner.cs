using System.Collections.Generic;
using Antique_Tycoon.Models.Effects;
using Antique_Tycoon.Models.Effects.StaffEffectImpls;
using Antique_Tycoon.Utilities;
using Avalonia.Media.Imaging;

namespace Antique_Tycoon.Models.Entities.StaffImpls;

public class MineOwner:IStaff
{
  public string Name => "村民";
  public List<IStaffEffect> Effects => [new MineManagerEffect()];
  public Bitmap Image => ImageHelper.GetBitmap("avares://Antique_Tycoon/Assets/Image/Staff/Villager.png");
  public decimal HiringCost { get; set; } = 1000;
  public Dictionary<int, int> HiringAntiqueCost => [];
  public decimal Salary { get; set; }
}