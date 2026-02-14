using System;
using System.Collections.Generic;
using Antique_Tycoon.Models.Effects;
using Antique_Tycoon.Models.Effects.StaffEffectImpls;
using Antique_Tycoon.Utilities;
using Avalonia.Media.Imaging;

namespace Antique_Tycoon.Models.Entities.StaffImpls;

public class WelfareCheat : IStaff
{
  public string Uuid { get; set; } = Guid.NewGuid().ToString();
  public string Name { get; set; } = "蜘蛛";
  public List<IStaffEffect> Effects => [new PassStartBonusEffect(300)];
  public Bitmap Image => ImageHelper.GetBitmap("avares://Antique_Tycoon/Assets/Image/Staff/Spider.png");
  public decimal HiringCost { get; set; } = 1000;
  public Dictionary<int, int> HiringAntiqueCost => [];
  public decimal Salary { get; set; }
  public string Description => "ppt创业者";
}