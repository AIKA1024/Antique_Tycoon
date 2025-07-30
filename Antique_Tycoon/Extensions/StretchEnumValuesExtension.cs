using System;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace Antique_Tycoon.Extensions;

public static class StretchEnumValuesExtension
{
  public static Stretch[] Values = [Stretch.None, Stretch.Fill, Stretch.Uniform, Stretch.UniformToFill];
}