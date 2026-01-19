using Antique_Tycoon.Models.Nodes;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Antique_Tycoon.Converters;

public static class EnumConverter
{
  public static readonly IValueConverter BonusTypeToString =
    new FuncValueConverter<BonusType, string>(type =>
    {
      return type switch
      {
        BonusType.FlatAdd => "加法",
        BonusType.Multiplier => "乘法",
        _ => type.ToString()
      };
    });
  
  public static readonly IValueConverter StretchToString =
    new FuncValueConverter<Stretch, string>(type =>
    {
      return type switch
      {
        Stretch.None => "不拉伸",
        Stretch.Fill => "填充",
        Stretch.Uniform => "均匀",
        Stretch.UniformToFill => "均匀填充",
        _ => type.ToString()
      };
    });
}