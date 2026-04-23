using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Antique_Tycoon.Models.Enums;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Nodes;

public partial class Estate : NodeModel
{
  [ObservableProperty] public partial decimal Value { get; set; }
  [ObservableProperty] public partial int CurrentLevel { get; set; } = 1;

  /// <summary>
  /// 每回合要交的税
  /// </summary>
  [ObservableProperty]
  public partial decimal PropertyTax { get; set; } = 50;

  public override string? Tooltip
  {
    get
    {
      if (!string.IsNullOrEmpty(field))
        return field;

      // 把每一项生成好，放到列表里
      var lines = new List<string>();
      for (int i = 0; i < RevenueModifiers.Count; i++)
      {
        var modifier = RevenueModifiers[i];
        var symbol = modifier.BonusType == BonusType.FlatAdd ? "+" : "*";
        lines.Add($"等级: {i + 1}   {symbol} {modifier.EffectNum}");
      }

      field = string.Join("\r\n", lines);
      return field;
    }
    set;
  }

  public ObservableCollection<BonusEffect> RevenueModifiers { get; set; } = [];

  public decimal CalculateCurrentRevenue(decimal baseValue)
  {
    // 安全检查：防止 Level 超出数组范围
    if (CurrentLevel - 1 >= RevenueModifiers.Count) return baseValue;

    var effect = RevenueModifiers[CurrentLevel - 1];

    return effect.BonusType switch
    {
      BonusType.FlatAdd => effect.EffectNum + baseValue,
      BonusType.Multiplier => effect.EffectNum * baseValue,
      _ => baseValue
    };
  }
}

public partial class BonusEffect(BonusType bonusType, decimal effectNum) : ObservableObject
{
  [ObservableProperty] public partial BonusType BonusType { get; set; } = bonusType;
  [ObservableProperty] public partial decimal EffectNum { get; set; } = effectNum;
}