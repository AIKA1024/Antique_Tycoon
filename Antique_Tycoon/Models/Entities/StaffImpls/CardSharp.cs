using System;
using System.Collections.Generic;
using Antique_Tycoon.Models.Effects;
using Antique_Tycoon.Models.Effects.StaffEffectImpls;
using Antique_Tycoon.Utilities;
using Avalonia.Media.Imaging;

namespace Antique_Tycoon.Models.Entities.StaffImpls;

public class CardSharp:IStaff
{
  public string Uuid { get; set; } = Guid.NewGuid().ToString();
  public string Name => "狐狸";
  public List<IStaffEffect> Effects => [new CheaterEffect()];
  public Bitmap Image => ImageHelper.GetBitmap("avares://Antique_Tycoon/Assets/Image/Staff/Fox.png");
  public decimal HiringCost => 1000;
  public Dictionary<int, int> HiringAntiqueCost => [];
  public decimal Salary => 0;
  public string Description => "这是狐狸，它很可爱";
}