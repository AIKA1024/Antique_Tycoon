using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Antique_Tycoon.Converters;

public class SolidBrushToColorConverter : MarkupExtension, IValueConverter
{
  public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is ISolidColorBrush brush)
      return brush.Color;
    return Colors.Transparent;
  }

  public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
  {
    if (value is Color color)
      return new SolidColorBrush(color);
    throw new NotSupportedException("Only a Color can be converted");
  }

  public override object ProvideValue(IServiceProvider serviceProvider)
  {
    return this;
  }
}