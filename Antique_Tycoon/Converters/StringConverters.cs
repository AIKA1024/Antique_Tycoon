using Avalonia.Data.Converters;

namespace Antique_Tycoon.Converters;

public static class StringConverters
{
  public static readonly IValueConverter Equal =
    new FuncValueConverter<object?, object?, bool>((a, b) => a?.ToString() == b?.ToString());
}