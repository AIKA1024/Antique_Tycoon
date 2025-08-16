using System.Linq;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Antique_Tycoon.Converters;

public static class MultiplyValuesConverter
{
  public static readonly IMultiValueConverter Multiply =
    new FuncMultiValueConverter<double, double>(values =>
    {
      var result = 1d;
      foreach (var value in values)
        result *= value;
      return result;
    });
}