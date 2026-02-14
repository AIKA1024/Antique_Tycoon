using System;
using System.Collections.Generic;
using Antique_Tycoon.Models.Effects;
using Antique_Tycoon.Models.Effects.StaffEffectImpls;
using Antique_Tycoon.Utilities;
using Avalonia.Media.Imaging;

namespace Antique_Tycoon.Models.Entities.StaffImpls;

public class TaxEvasionKing:IStaff
{
  public string Uuid { get; set; } = Guid.NewGuid().ToString();
  public string Name { get; set; } = "漏税王";
  public List<IStaffEffect> Effects { get; set; } = [new TaxEvasionEffect()];
  public Bitmap Image => ImageHelper.GetBitmap("avares://Antique_Tycoon/Assets/Image/Staff/Spider.png");
  public decimal HiringCost { get; set; } = 1000;
  public Dictionary<int, int> HiringAntiqueCost => [];
  public decimal Salary { get; set; }
  public string Description => "我只是忘记了";
}