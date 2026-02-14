using System;
using System.Collections.Generic;
using Antique_Tycoon.Models.Effects;
using Antique_Tycoon.Models.Effects.StaffEffectImpls;
using Antique_Tycoon.Utilities;
using Avalonia.Media.Imaging;

namespace Antique_Tycoon.Models.Entities.StaffImpls;

public class TaxLord:IStaff
{
  public string Uuid { get; set; } = Guid.NewGuid().ToString();
  
  public string Name => "万税爷";
  public List<IStaffEffect> Effects => [new TaxHikeEffect()];
  public Bitmap Image => ImageHelper.GetBitmap("avares://Antique_Tycoon/Assets/Image/Staff/Trump.png");
  public decimal HiringCost { get; } = 1000;
  public Dictionary<int, int> HiringAntiqueCost => [];
  public decimal Salary { get; } = 500;
  public string Description => "统统加税！";
}