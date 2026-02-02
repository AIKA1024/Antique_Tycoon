using System.Collections.Generic;
using Antique_Tycoon.Models.Effects;
using Avalonia.Media.Imaging;

namespace Antique_Tycoon.Models.Entities;

public interface IStaff
{
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
}