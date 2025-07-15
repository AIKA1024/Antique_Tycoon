using System;
using System.Linq;
using Antique_Tycoon.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Xaml.Interactivity;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Behaviors;

public class ShowFlyoutBehavior : Behavior<Control>
{
  private Point _oldPoint;
  private Flyout _flyout;

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
      menuItem.Click+= MenuItemOnClick;
  }

  private void MenuItemOnClick(object? sender, RoutedEventArgs e)
  {
    _flyout.Hide();
  }

  override protected void OnDetaching()
  {
    base.OnDetaching();
    AssociatedObject.PointerPressed -= OnPointerPressed;
    AssociatedObject.PointerReleased -= OnPointerReleased;
    AssociatedObject.Initialized += AssociatedObjectOnLoaded;
    foreach (var menuItem in ((Panel)_flyout.Content).Children.OfType<MenuItem>())
      menuItem.Click -= MenuItemOnClick;
  }

  private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
  {
    var pointer = e.GetCurrentPoint(sender as Visual);
    if (pointer.Properties.IsRightButtonPressed)
    {
      _oldPoint = e.GetPosition(App.Current.Services.GetRequiredService<MainWindow>());
    }
  }

  private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
  {
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