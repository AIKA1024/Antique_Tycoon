using System;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Models.Node;
using Antique_Tycoon.ViewModels;
using Antique_Tycoon.Views.Controls;
using Antique_Tycoon.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Behaviors;

public partial class CanvasItemDragBehavior : Behavior<Control> //想着解耦把逻辑放在行为里，但又不得不依赖vm的属性💩
{
  private NodeModel _model;
  private bool _isDragging;
  private Point _lastPointerPosition;
  [GeneratedDirectProperty]
  public partial double Scale { get; set; }
  
  [GeneratedDirectProperty]
  public partial Point Offset { get; set; }

  protected override void OnAttached()
  {
    base.OnAttached();
    AssociatedObject.Loaded += AssociatedObjectOnLoaded;
    AssociatedObject.PointerPressed += AssociatedObjectOnPointerPressed;
    AssociatedObject.PointerReleased += AssociatedObjectOnPointerReleased;
    AssociatedObject.PointerMoved += AssociatedObjectOnPointerMoved;
    AssociatedObject.SizeChanged += AssociatedObjectOnSizeChanged;
  }

  private void AssociatedObjectOnSizeChanged(object? sender, SizeChangedEventArgs e) =>
    LayoutChanged.RaiseLayoutChanged(AssociatedObject);

  protected override void OnDetaching()
  {
    base.OnDetaching();
    AssociatedObject.PointerPressed -= AssociatedObjectOnPointerPressed;
    AssociatedObject.PointerReleased -= AssociatedObjectOnPointerReleased;
    AssociatedObject.PointerMoved -= AssociatedObjectOnPointerMoved;
    AssociatedObject.Loaded -= AssociatedObjectOnLoaded;
    AssociatedObject.SizeChanged -= AssociatedObjectOnSizeChanged;
  }

  private void AssociatedObjectOnLoaded(object? sender, RoutedEventArgs e)
  {
    if (AssociatedObject.DataContext is NodeModel model)
      _model = model;
    else
      throw new Exception("只能依附在数据上下文为CanvasEntity的元素上");
  }

  private void AssociatedObjectOnPointerPressed(object? sender, PointerPressedEventArgs e)
  {
    if (e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed)
    {
      _isDragging = true;
      _lastPointerPosition = e.GetPosition(App.Current.Services.GetRequiredService<MainWindow>());
      e.Pointer.Capture(AssociatedObject);
    }
  }

  private void AssociatedObjectOnPointerMoved(object? sender, PointerEventArgs e)
  {
    if (!_isDragging) return;

    var currentPointerPosition = e.GetPosition(App.Current.Services.GetRequiredService<MainWindow>());
    var delta = currentPointerPosition - _lastPointerPosition;
    var adjustedDelta = new Point(delta.X / Scale, delta.Y / Scale);

    // 增量叠加 每次移动10个单位
    var snappedDeltaX = Math.Round(adjustedDelta.X / 10) * 10;
    var snappedDeltaY = Math.Round(adjustedDelta.Y / 10) * 10;

    if (snappedDeltaX != 0 || snappedDeltaY != 0)
    {
      _model.Left += snappedDeltaX;
      _model.Top += snappedDeltaY;
      _lastPointerPosition = new Point(
        _lastPointerPosition.X + snappedDeltaX * Scale,
        _lastPointerPosition.Y + snappedDeltaY * Scale
      );
      LayoutChanged.RaiseLayoutChanged(AssociatedObject);
    }
  }

  private void AssociatedObjectOnPointerReleased(object? sender, PointerReleasedEventArgs e)
  {
    _isDragging = false;
    e.Pointer.Capture(null);
  }
}