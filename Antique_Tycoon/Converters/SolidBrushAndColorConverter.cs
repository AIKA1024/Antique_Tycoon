using System;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Antique_Tycoon.Converters;

public static class SolidBrushAndColorConverter
{
  public static readonly IValueConverter ToColor =
    new FuncValueConverter<object?, Color>((a) =>
    {
      if (a is ISolidColorBrush brush)
        return brush.Color;
      return Colors.Transparent;
    });
  
  public static readonly IValueConverter ToSolidBrush =
    new FuncValueConverter<object?, SolidColorBrush>((a) =>
    {
      if (a is Color color)
        return new SolidColorBrush(color);
      throw new NotSupportedException("Only a Color can be converted");
    });
}