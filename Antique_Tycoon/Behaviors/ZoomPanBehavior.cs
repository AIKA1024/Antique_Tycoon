using System;
using System.Windows.Input;
using Antique_Tycoon.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;

namespace Antique_Tycoon.Behaviors;

public class ZoomPanBehavior : Behavior<Panel>
{
  private Point? _lastPointer;
  private GamePageViewModel? _vm;

  protected override void OnAttached()
  {
    base.OnAttached();
    if (AssociatedObject is { } panel)
    {
      panel.PointerPressed += OnPointerPressed;
      panel.PointerMoved += OnPointerMoved;
      panel.PointerReleased += OnPointerReleased;
      panel.PointerWheelChanged += OnPointerWheelChanged;
      panel.DataContextChanged += OnDataContextChanged;
      // 如果已经有 DataContext，也立即尝试赋值
      TryBindViewModel(panel.DataContext);
    }
  }

  private void TryBindViewModel(object? dc)
  {
    if (dc is GamePageViewModel vm)
      _vm = vm;
  }

  private void OnDataContextChanged(object? sender, EventArgs e)
  {
    if (sender is Panel p)
      TryBindViewModel(p.DataContext);
  }

  protected override void OnDetaching()
  {
    base.OnDetaching();
    if (AssociatedObject is { } panel)
    {
      panel.PointerPressed -= OnPointerPressed;
      panel.PointerMoved -= OnPointerMoved;
      panel.PointerReleased -= OnPointerReleased;
      panel.PointerWheelChanged -= OnPointerWheelChanged;
      panel.DataContextChanged -= OnDataContextChanged;
    }
  }

  private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
  {
    if (e.GetCurrentPoint(AssociatedObject).Properties.IsRightButtonPressed)
      _lastPointer = e.GetPosition(AssociatedObject);
  }

  private void OnPointerMoved(object? sender, PointerEventArgs e)
  {
    if (_lastPointer != null && e.GetCurrentPoint(AssociatedObject).Properties.IsRightButtonPressed)
    {
      var pos = e.GetPosition(AssociatedObject);
      var delta = pos - _lastPointer.Value;
      _vm!.Offset = new Point(_vm.Offset.X + delta.X, _vm.Offset.Y + delta.Y);
      _lastPointer = pos;
    }
  }

  private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
  {
    _lastPointer = null;
  }

  private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
  {
    if (_vm == null || AssociatedObject == null) return;
    var pos = e.GetPosition(AssociatedObject);
    var delta = e.Delta.Y > 0 ? 1.1 : 0.9; // 改用乘数而非增量
    var newScale = Math.Clamp(_vm.Scale * delta, 0.2, 3.0);

    // 更稳定的缩放中心计算
    _vm.Offset = new Point(
      pos.X - (pos.X - _vm.Offset.X) * (newScale / _vm.Scale),
      pos.Y - (pos.Y - _vm.Offset.Y) * (newScale / _vm.Scale)
    );
    _vm.Scale = newScale;
  }
}