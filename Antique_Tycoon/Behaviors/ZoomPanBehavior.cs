using System;
using System.Windows.Input;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.ViewModels;
using Antique_Tycoon.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.Xaml.Interactivity;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Behaviors;

public class ZoomPanBehavior : Behavior<Control> //todo 如果控件上有其他控件，拖拽的控件就收不到OnPointerPressed事件
{
  private Point? _lastPointer;
  private DragAndZoomViewModel? _vm;

  private static readonly Cursor Hand =
    new(new Bitmap(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/DragHand.png"))), new PixelPoint(8, 8));

  protected override void OnAttached()
  {
    base.OnAttached();
    if (AssociatedObject is { } control)
    {
      control.PointerPressed += OnPointerPressed;
      control.PointerMoved += OnPointerMoved;
      control.PointerReleased += OnPointerReleased;
      control.PointerWheelChanged += OnPointerWheelChanged;
      control.DataContextChanged += OnDataContextChanged;
      // 如果已经有 DataContext，也立即尝试赋值
      TryBindViewModel(control.DataContext);
    }
  }

  private void TryBindViewModel(object? dataContext)
  {
    if (dataContext is DragAndZoomViewModel vm)
      _vm = vm;
  }

  private void OnDataContextChanged(object? sender, EventArgs e)
  {
    if (sender is Control c)
      TryBindViewModel(c.DataContext);
  }

  protected override void OnDetaching()
  {
    base.OnDetaching();
    if (AssociatedObject is { } control)
    {
      control.PointerPressed -= OnPointerPressed;
      control.PointerMoved -= OnPointerMoved;
      control.PointerReleased -= OnPointerReleased;
      control.PointerWheelChanged -= OnPointerWheelChanged;
      control.DataContextChanged -= OnDataContextChanged;
    }
  }

  private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
  {
    if (e.GetCurrentPoint(AssociatedObject).Properties.IsRightButtonPressed)
    {
      _lastPointer = e.GetPosition(AssociatedObject);
    }
  }

  private void OnPointerMoved(object? sender, PointerEventArgs e)
  {
    if (_lastPointer != null && e.GetCurrentPoint(AssociatedObject).Properties.IsRightButtonPressed)
    {
      var pos = e.GetPosition(AssociatedObject);
      var delta = pos - _lastPointer.Value;
      _vm!.Offset = new Point(_vm.Offset.X + delta.X, _vm.Offset.Y + delta.Y);
      _lastPointer = pos;
      ((Control)sender).Cursor = Hand;
    }
  }

  private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
  {
    _lastPointer = null;
    ((Control)sender).Cursor = Cursor.Default;
  }

  private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
  {
    if (_vm == null || AssociatedObject == null) return;
    var pos = e.GetPosition(AssociatedObject);
    var delta = e.Delta.Y > 0 ? 1.1 : 0.9; // 改用乘数而非增量
    var newScale = Math.Clamp(_vm.Scale * delta, 0.4, 3);

    // 更稳定的缩放中心计算  设置画布大小后鼠标缩放错误，先禁用
    _vm.Offset = new Point(
      pos.X - (pos.X - _vm.Offset.X) * (newScale / _vm.Scale),
      pos.Y - (pos.Y - _vm.Offset.Y) * (newScale / _vm.Scale)
    );
    _vm.Scale = newScale;
  }
}