using System;
using System.Globalization;
using Avalonia;
using Avalonia.Data.Converters;

namespace Antique_Tycoon.Converters;

public class MultiplyConverter
{
  // 提供一个静态单例，方便在 XAML 里通过 x:Static 直接使用
  public static readonly IValueConverter Multiply =
    new FuncValueConverter<double, string, double>((a, b) =>
    {
      if (double.TryParse(b, out var result))
      {
        return result * a;
      }
      return a;
    });
}