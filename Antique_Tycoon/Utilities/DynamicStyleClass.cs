using Avalonia;
using Avalonia.Controls;

namespace Antique_Tycoon.Utilities;

public class DynamicStyleClass
{
  // 定义一个公开的附加属性：UseDataContextTypeName
  public static readonly AttachedProperty<bool> UseDataContextTypeNameProperty =
    AvaloniaProperty.RegisterAttached<Control, bool>(
      "UseDataContextTypeName", typeof(DynamicStyleClass));

  public static void SetUseDataContextTypeName(AvaloniaObject element, bool value)
  {
    element.SetValue(UseDataContextTypeNameProperty, value);
  }

  public static bool GetUseDataContextTypeName(AvaloniaObject element)
  {
    return element.GetValue(UseDataContextTypeNameProperty);
  }

  // 定义一个私有的附加属性，用于记录上一次添加的类名，方便在 DataContext 改变时移除它
  private static readonly AttachedProperty<string?> LastAddedClassProperty =
    AvaloniaProperty.RegisterAttached<Control, string?>(
      "LastAddedClass", typeof(DynamicStyleClass));

  static DynamicStyleClass()
  {
    // 监听附加属性本身的开启/关闭状态
    UseDataContextTypeNameProperty.Changed.AddClassHandler<Control>(OnPropertyChanged);
    // 监听控件 DataContext 的变化
    StyledElement.DataContextProperty.Changed.AddClassHandler<Control>(OnDataContextChanged);
  }

  private static void OnPropertyChanged(Control control, AvaloniaPropertyChangedEventArgs e)
  {
    if (e.NewValue is bool isEnabled && isEnabled)
    {
      UpdateClass(control, control.DataContext);
    }
    else
    {
      RemoveLastClass(control);
    }
  }

  private static void OnDataContextChanged(Control control, AvaloniaPropertyChangedEventArgs e)
  {
    // 只有当启用了该附加属性时，才处理 DataContext 变化
    if (GetUseDataContextTypeName(control))
    {
      UpdateClass(control, e.NewValue);
    }
  }

  private static void UpdateClass(Control control, object? dataContext)
  {
    RemoveLastClass(control);

    if (dataContext != null)
    {
      // 获取当前 DataContext 的类型名称（例如 "MainViewModel"）
      string newClass = dataContext.GetType().Name;

      // 将类型名称加入到控件的 Classes 集合中
      control.Classes.Add(newClass);

      // 记录下来，以便下次移除
      control.SetValue(LastAddedClassProperty, newClass);
    }
  }

  private static void RemoveLastClass(Control control)
  {
    var lastClass = control.GetValue(LastAddedClassProperty);
    if (!string.IsNullOrEmpty(lastClass))
    {
      control.Classes.Remove(lastClass);
      control.SetValue(LastAddedClassProperty, null);
    }
  }
}