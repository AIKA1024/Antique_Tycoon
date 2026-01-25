using System.Collections.Generic;
using Antique_Tycoon.Models.Effects;

namespace Antique_Tycoon.Models.Entities;

public interface IStaff
{
  public string Name { get; }
  public List<IStaffEffect> Effects { get; }

  /// <summary>
  /// 雇佣价
  /// </summary>
  public decimal HiringCost { get; }
  
  /// <summary>
  /// 每回合要发放的薪水
  /// </summary>
  public decimal Salary { get; }
}