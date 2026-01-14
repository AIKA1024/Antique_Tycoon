using System.Collections.Generic;
using System.Diagnostics;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Controls.Shapes;
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

    var wrapper = new Panel(); //防止SetAdornedElement导致动画失效，多一层包装
    AdornerLayer.SetAdornedElement(wrapper, target);

    var adorner = new ContentControl
    {
      Content = content,
      IsHitTestVisible = false,
      ClipToBounds = false,
    };

    var trianglePath = new Path
    {
      Fill = new SolidColorBrush(Color.Parse("#ECCC68")), // 三角颜色和气泡一致，无缝衔接无割裂感
      Stretch = Stretch.Fill,
      Width = 16, // 三角宽度，按需调整
      Height = 8, // 三角高度，按需调整
      Margin = new Thickness(0, -1, 0, 0),
      HorizontalAlignment = HorizontalAlignment.Center, // 水平居中
      VerticalAlignment = VerticalAlignment.Center,
      Data = Geometry.Parse("M0,0 L8,8 L16,0 Z")
    };

    var stackPanel = new StackPanel();
    stackPanel.Children.Add(adorner);
    stackPanel.Children.Add(trianglePath);
    stackPanel.Margin = new Thickness(0, -target.Bounds.Height * 1.5, 0, 0);

    wrapper.Children.Add(stackPanel);
    AdornerLayer.SetIsClipEnabled(wrapper, false);

    layer.Children.Add(wrapper);
    Adorners[target] = wrapper;
    stackPanel.Classes.Add("CardAppear");
  }
}