using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Antique_Tycoon.Models.Nodes;

public partial class Estate : NodeModel
{
  public int Value { get; set; }
  public int Level { get; set; } = 1;
  public ObservableCollection<int> RevenueModifiers { get; } = [];
  public BonusType BonusType { get; set; } = BonusType.FlatAdd;
  
  public int CalculateCurrentRevenue(int baseValue)
  {
    // 安全检查：防止 Level 超出数组范围
    if (Level - 1 >= RevenueModifiers.Count) return baseValue;

    var effect = RevenueModifiers[Level - 1];

    return BonusType switch
    {
      BonusType.FlatAdd => effect + baseValue,
      BonusType.Multiplier => effect * baseValue,
      _ => baseValue
    };
  }
}

public enum BonusType
{
  FlatAdd,
  Multiplier
}
