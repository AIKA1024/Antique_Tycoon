using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Antique_Tycoon.Converters;

public static class RarityColorConverter
{
  // 缓存画笔，避免重复创建
  private static readonly Dictionary<int, IBrush> BrushCache = new();

  static RarityColorConverter()
  {
    AddColor(2, "#3DDD4D"); // 绿 (Green)
    AddColor(3, "#23323D"); // 蓝 (Blue)
    AddColor(4, "#262533"); // 紫 (Purple)
    AddColor(5, "#DDAC3D"); // 金 (Gold)
    AddColor(6, "#DD653D"); // 橙 (Orange)
    AddColor(7, "#3F2629"); // 红 (Red)

    // 默认颜色 (普通/白)
    AddColor(0, "#DDDDDD");
  }

  private static void AddColor(int key, string hex)
  {
    // Avalonia 解析颜色的方式
    var color = Color.Parse(hex);
    var brush = new SolidColorBrush(color);
    BrushCache[key] = brush;
  }

  public static readonly IValueConverter DiceToBrush =
    new FuncValueConverter<int, IBrush>(GetBrush);
  
  public static IBrush GetBrush(int key)
  {
    return BrushCache.TryGetValue(key, out var brush) ? brush : BrushCache[0];
  }
}