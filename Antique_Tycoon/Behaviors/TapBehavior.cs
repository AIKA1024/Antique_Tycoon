using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Antique_Tycoon.Behaviors;

public class TapBehavior: AvaloniaObject
{
  static TapBehavior()
  {
    CommandProperty.Changed.AddClassHandler<InputElement>(HandleCommandChanged);
  }

  public static readonly AttachedProperty<ICommand?> CommandProperty =
    AvaloniaProperty.RegisterAttached<TapBehavior,InputElement,ICommand?>(
      "Command");
  
  public static readonly AttachedProperty<object?> CommandParameterProperty = AvaloniaProperty.RegisterAttached<TapBehavior, InputElement, object?>(
    "CommandParameter");

  public static ICommand? GetCommand(AvaloniaObject element)
  {
    return element.GetValue(CommandProperty);
  }

  public static void SetCommand(AvaloniaObject element, ICommand value)
  {
    element.SetValue(CommandProperty, value);
  }
  
  public static void SetCommandParameter(AvaloniaObject element, object? parameter)
  {
    element.SetValue(CommandParameterProperty, parameter);
  }

  public static object? GetCommandParameter(AvaloniaObject element)
  {
    return element.GetValue(CommandParameterProperty);
  }
  
  private static void HandleCommandChanged(InputElement inputElement, AvaloniaPropertyChangedEventArgs e)
  {
    if (e.NewValue is ICommand commandValue)
    {
      // 添加非空值
      inputElement.AddHandler(InputElement.TappedEvent, Handler);
    }
    else
    {
      // 删除之前的值
      inputElement.RemoveHandler(InputElement.TappedEvent, Handler);
    }
  }
  private static void Handler(object? s, TappedEventArgs e)
  {
    if (s is InputElement inputElement)
    {
      var commandParameter = inputElement.GetValue(CommandParameterProperty);
      var commandValue = inputElement.GetValue(CommandProperty);
      if (commandValue?.CanExecute(commandParameter) == true)
      {
        commandValue.Execute(commandParameter);
      }
    }
  }
}