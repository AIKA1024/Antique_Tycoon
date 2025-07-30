using System;
using Avalonia.Markup.Xaml;

namespace Antique_Tycoon.Extensions;

public class EnumValuesExtension: MarkupExtension
{
  public Type? Type { get; set; }

  public override object ProvideValue(IServiceProvider serviceProvider)
  {
    if (Type is null)
      throw new InvalidOperationException("EnumValuesExtension: Type cannot be null.");

    if (!Type.IsEnum)
      throw new InvalidOperationException("EnumValuesExtension: Type must be an enum.");

    return Enum.GetValues(Type);
  }
}