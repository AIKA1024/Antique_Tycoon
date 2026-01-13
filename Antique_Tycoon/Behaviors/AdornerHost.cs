using System.Collections.Generic;
using System.Diagnostics;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Behaviors;

public static class AdornerHost
{
  private static readonly Dictionary<Control, Control> Adorners = new();

  public static readonly AttachedProperty<object?> AdornerContentProperty =
    AvaloniaProperty.RegisterAttached<Control, Control, object?>(
      "AdornerContent");

  public static void SetAdornerContent(Control element, object? value) =>
    element.SetValue(AdornerContentProperty, value);

  static AdornerHost()
  {
    AdornerContentProperty.Changed.AddClassHandler<Control>(OnAdornerChanged);
  }

  private static void OnAdornerChanged(Control target, AvaloniaPropertyChangedEventArgs e)
  {
    // var layer = AdornerLayer.GetAdornerLayer(target);
    //使用自己放的AdornerLayer，可以让游戏ui在这些装饰器上面
    var layer = target.GetParentOfType<Grid>("MainGrid").GetChildOfType<AdornerLayer>("AdornerLayer");

    // 1️⃣ 如果已经有旧的 Adorner，先移除
    if (Adorners.TryGetValue(target, out var oldAdorner))
    {
      layer.Children.Remove(oldAdorner);
      Adorners.Remove(target);
    }

    if (e.NewValue is not object content)
      return;

    var wrapper = new Panel();//防止SetAdornedElement导致动画失效，多一层包装
    AdornerLayer.SetAdornedElement(wrapper, target);
    
    var adorner = new ContentControl
    {
      Content = content,
      IsHitTestVisible = false,
      ClipToBounds = false,
      Margin = new Thickness(0, -target.Bounds.Height * 2 - 100, 0, 0),
    };
    wrapper.Children.Add(adorner);
    AdornerLayer.SetIsClipEnabled(wrapper, false);

    layer.Children.Add(wrapper);
    Adorners[target] = wrapper;
    adorner.Classes.Add("CardAppear");
    Debug.WriteLine("success");
  }
}