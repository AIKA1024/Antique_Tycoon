using System;
using System.Linq;
using System.Windows.Input;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.ViewModels;
using Antique_Tycoon.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Behaviors;

public partial class ZoomPanBehavior : Behavior<Control> //todo 如果控件上有其他控件，拖拽的控件就收不到OnPointerPressed事件
{
  private Point? _lastPointer;

  private static readonly Cursor Hand =
    new(new Bitmap(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/DragHand.png"))),
      new PixelPoint(8, 8));

  [GeneratedDirectProperty] public partial double Scale { get; set; }

  [GeneratedDirectProperty] public partial Point Offset { get; set; }


  protected override void OnAttached()
  {
    base.OnAttached();
    if (AssociatedObject is { } control)
    {
      control.PointerPressed += OnPointerPressed;
      control.PointerMoved += OnPointerMoved;
      control.PointerReleased += OnPointerReleased;
      control.PointerWheelChanged += OnPointerWheelChanged;
      control.Loaded += ControlOnLoaded;
    }
  }

  private void ControlOnLoaded(object? sender, RoutedEventArgs e)
  {
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
      control.Loaded -= ControlOnLoaded;
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
    if (_lastPointer == null || !e.GetCurrentPoint(AssociatedObject).Properties.IsRightButtonPressed) return;
    var pos = e.GetPosition(AssociatedObject);
    var delta = pos - _lastPointer.Value;
    Offset = new Point(Offset.X + delta.X, Offset.Y + delta.Y);
    _lastPointer = pos;
    ((Control)sender).Cursor = Hand;
  }

  private void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
  {
    _lastPointer = null;
    ((Control)sender).Cursor = Cursor.Default;
  }

  private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
  {
    if (AssociatedObject == null) return;
    var pos = e.GetPosition(AssociatedObject);
    var delta = e.Delta.Y > 0 ? 1.1 : 0.9; // 改用乘数而非增量
    var newScale = Math.Clamp(Scale * delta, 0.3, 3);

    Offset = new Point(
      pos.X - (pos.X - Offset.X) * (newScale / Scale),
      pos.Y - (pos.Y - Offset.Y) * (newScale / Scale)
    );
    Scale = newScale;
  }
}