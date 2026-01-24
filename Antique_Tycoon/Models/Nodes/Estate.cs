using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Nodes;

public partial class Estate : NodeModel
{
  public int Value { get; set; }
  [ObservableProperty]
  public partial int Level { get; set; } = 1;
  public ObservableCollection<BonusEffect> RevenueModifiers { get; set; } = [];

  public int CalculateCurrentRevenue(int baseValue)
  {
    // 安全检查：防止 Level 超出数组范围
    if (Level - 1 >= RevenueModifiers.Count) return baseValue;

    var effect = RevenueModifiers[Level - 1];

    return effect.BonusType switch
    {
      BonusType.FlatAdd => effect.EffectNum + baseValue,
      BonusType.Multiplier => effect.EffectNum * baseValue,
      _ => baseValue
    };
  }
}

public partial class BonusEffect(BonusType bonusType, int effectNum) : ObservableObject
{
  [ObservableProperty] public partial BonusType BonusType { get; set; } = bonusType;
  [ObservableProperty] public partial int EffectNum { get; set; } = effectNum;
}

public enum BonusType
{
  FlatAdd,
  Multiplier
}