using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using Antique_Tycoon.Models.Enums;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Nodes;

public partial class Estate : NodeModel
{
  [ObservableProperty] public partial decimal Value { get; set; }
  [ObservableProperty] public partial int CurrentLevel { get; set; } = 0;

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

      var lines = new List<string>(RevenueModifiers.Count + 1); // 预分配容量
      for (int i = 0; i < RevenueModifiers.Count; i++)
      {
        var modifier = RevenueModifiers[i];
        var symbol = modifier.BonusType == BonusType.FlatAdd ? "+" : "×";
        lines.Add($"等级: {i + 1}  收益： {symbol} {modifier.EffectNum}");
      }

      lines.Add($"每回合税:{PropertyTax}");

      return string.Join(Environment.NewLine, lines);
    }
    set;
  }

  public ObservableCollection<BonusEffect> RevenueModifiers { get; set; } = [];

  public decimal CalculateCurrentRevenue(decimal baseValue)
  {
    // 安全检查：防止 Level 超出数组范围
    var index = CurrentLevel - 1;

    if (index >= RevenueModifiers.Count || index < 0) return baseValue;

    var effect = RevenueModifiers[CurrentLevel];

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