using System.Collections.Generic;
using System.Text.Json.Serialization;
using Antique_Tycoon.Models.Effects;
using Antique_Tycoon.Models.Entities.StaffImpls;
using Avalonia.Media.Imaging;

namespace Antique_Tycoon.Models.Entities;

[JsonDerivedType(typeof(CardSharp), "CardSharp")]
[JsonDerivedType(typeof(MineOwner), "MineOwner")]
[JsonDerivedType(typeof(TaxEvasionKing), "TaxEvasionKing")]
[JsonDerivedType(typeof(TaxLord), "TaxLord")]
[JsonDerivedType(typeof(WelfareCheat), "WelfareCheat")]
public interface IStaff
{
  public string Uuid { get; set; }
  public string Name { get; }
  public List<IStaffEffect> Effects { get; }
  
  public Bitmap Image { get; }

  /// <summary>
  /// 雇佣价
  /// </summary>
  public decimal HiringCost { get; }
  /// <summary>
  /// 额外的古董需求 key为自定义的古董的index
  /// </summary>
  public Dictionary<int, int> HiringAntiqueCost { get; }
  
  /// <summary>
  /// 每回合要发放的薪水
  /// </summary>
  public decimal Salary { get; }
  
  public string Description { get; }
}