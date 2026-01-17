using System;
using Avalonia.Markup.Xaml;

namespace Antique_Tycoon.Extensions;

public class EnumItemsExtension : MarkupExtension
{
  private readonly Type _enumType;

  // 构造函数，接收 XAML 传进来的枚举类型
  public EnumItemsExtension(Type enumType)
  {
    if (enumType is null || !enumType.IsEnum)
      throw new ArgumentException("Type must be an enum", nameof(enumType));
        
    _enumType = enumType;
  }

  // Avalonia 渲染时会调用这个方法获取数据
  public override object ProvideValue(IServiceProvider serviceProvider)
  {
    return Enum.GetValues(_enumType);
  }
}