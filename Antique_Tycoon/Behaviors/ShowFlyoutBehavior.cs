using System;
using System.Linq;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Behaviors;

public class ShowFlyoutBehavior : Behavior<Control>
{
  private Point _oldPoint;//用于判断释放拖拽了画布
  private Flyout _flyout;
  private Point _lastPointerPosition;//用于获取鼠标在canvas的坐标
  
  public static readonly AttachedProperty<Point?> PointerPositionProperty =
    AvaloniaProperty.RegisterAttached<ShowFlyoutBehavior, Control, Point?>(
      "PointerPosition");

  public static void SetPointerPosition(Control element, Point? value) =>
    element.SetValue(PointerPositionProperty, value);

  public static Point? GetPointerPosition(Control element) =>
    element.GetValue(PointerPositionProperty);

  protected override void OnAttached()
  {
    base.OnAttached();
    AssociatedObject.PointerPressed += OnPointerPressed;
    AssociatedObject.PointerReleased += OnPointerReleased;
    AssociatedObject.Loaded += AssociatedObjectOnLoaded;
  }

  private void AssociatedObjectOnLoaded(object? sender, EventArgs e)
  {
    _flyout = (Flyout)FlyoutBase.GetAttachedFlyout(AssociatedObject);
    foreach (var menuItem in ((Panel)_flyout.Content).Children.OfType<MenuItem>())
      menuItem.Tapped += MenuItemOnTap;
  }

  private void MenuItemOnTap(object? sender, TappedEventArgs e)
  {
    _flyout.Hide();
    SetPointerPosition(AssociatedObject, _lastPointerPosition);
  }

  override protected void OnDetaching()
  {
    base.OnDetaching();
    AssociatedObject.PointerPressed -= OnPointerPressed;
    AssociatedObject.PointerReleased -= OnPointerReleased;
    AssociatedObject.Initialized += AssociatedObjectOnLoaded;
    foreach (var menuItem in ((Panel)_flyout.Content).Children.OfType<MenuItem>())
      menuItem.Tapped -= MenuItemOnTap;
  }

  private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
  {
    if (sender!=e.Source) return;
    var pointer = e.GetCurrentPoint(sender as Visual);
    if (pointer.Properties.IsRightButtonPressed)
    {
      _oldPoint = e.GetPosition(App.Current.Services.GetRequiredService<MainWindow>());
      _lastPointerPosition = AssociatedObject.GetPointerPosition(e).SnapToGrid(10);
    }
  }

  private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
  {
    if (sender!=e.Source) return;
    if (e.InitialPressMouseButton != MouseButton.Right) return;
    var newPoint = e.GetPosition(App.Current.Services.GetRequiredService<MainWindow>());
    var dx = newPoint.X - _oldPoint.X;
    var dy = newPoint.Y - _oldPoint.Y;
    var distanceSquared = dx * dx + dy * dy;
    if (distanceSquared < 9) // 3 的平方
    {
      if (sender is Control ctl)
      {
        var flyout = (Flyout)FlyoutBase.GetAttachedFlyout(ctl);
        flyout.ShowAt(ctl, true);
      }
    }
  }
}