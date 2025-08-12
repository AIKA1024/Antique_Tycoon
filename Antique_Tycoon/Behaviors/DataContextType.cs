using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Input;

namespace Antique_Tycoon.Behaviors;

/// <summary>
/// 由于具体逻辑，因此无复用性
/// </summary>
public class DataContextType : AvaloniaObject
{
  static DataContextType()
  {
    TypeProperty.Changed.AddClassHandler<InputElement>((inputElement, e) =>
    {
      // 当 TypeProperty 的值改变时，这个回调会被触发
      var typeName = GetInheritanceChainString(inputElement.DataContext?.GetType());
      if (string.IsNullOrEmpty(typeName)) return;
      if (typeName.Contains("NodeModel"))
        inputElement.Classes.Add("NodeModel");
      else if (typeName.Contains("Connection"))
        inputElement.Classes.Add("Connection");
    });
  }

  public static readonly AttachedProperty<string> TypeProperty =
    AvaloniaProperty.RegisterAttached<DataContextType, InputElement, string>(
      "Type", "");

  public static string GetType(AvaloniaObject element) => element.GetValue(TypeProperty);
  public static void SetType(AvaloniaObject element, object value) => element.SetValue(TypeProperty, value);
  
  private static string GetInheritanceChainString(Type? type)
  {
    if (type == null)
    {
      return string.Empty;
    }

    var types = new List<string>();
    Type currentType = type;

    while (currentType != null && currentType != typeof(object))
    {
      types.Add(currentType.Name);
      currentType = currentType.BaseType;
    }
        
    // 按照继承顺序倒序排列（从子类到父类）
    types.Reverse();

    // 使用 "->" 符号连接所有类型名称
    return string.Join('.', types);
  }
}