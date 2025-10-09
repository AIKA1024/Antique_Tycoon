using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Markup.Xaml;

namespace Antique_Tycoon.Converters;

public class MultiValueToTupleConverter:MarkupExtension,IMultiValueConverter
{
  public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
  {
    return values.Count switch
    {
      1 => (values[0]), // 单元素元组 (T1)
      2 => (values[0], values[1]), // 双元素元组 (T1, T2)
      3 => (values[0], values[1], values[2]), // 三元素元组 (T1, T2, T3)
      4 => (values[0], values[1], values[2], values[3]),
      5 => (values[0], values[1], values[2], values[3], values[4]),
      6 => (values[0], values[1], values[2], values[3], values[4], values[5]),
      7 => (values[0], values[1], values[2], values[3], values[4], values[5],values[6]),
      // 按需扩展更多长度...
      _ => throw new NotSupportedException($"不支持 {values.Count} 个元素的元组转换")
    };
  }

  public override object ProvideValue(IServiceProvider serviceProvider)
  {
    return this;
  }
}