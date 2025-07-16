using System;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Models.Cell;
using Antique_Tycoon.ViewModels;
using Antique_Tycoon.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Behaviors;

public class CanvasItemDragBehavior : Behavior<Control>
{
  private CanvasEntity _model;
  private DragAndZoomViewModel dragAndZoomViewModel;
  private Point _dragOffset;
  private bool _isDragging;
  private Point _lastPointerPosition;

  protected override void OnAttached()
  {
    base.OnAttached();
    AssociatedObject.Loaded += AssociatedObjectOnLoaded;
    AssociatedObject.PointerPressed += AssociatedObjectOnPointerPressed;
    AssociatedObject.PointerReleased += AssociatedObjectOnPointerReleased;
    AssociatedObject.PointerMoved += AssociatedObjectOnPointerMoved;
  }

  protected override void OnDetaching()
  {
    base.OnDetaching();
    AssociatedObject.PointerPressed -= AssociatedObjectOnPointerPressed;
    AssociatedObject.PointerReleased -= AssociatedObjectOnPointerReleased;
    AssociatedObject.PointerMoved -= AssociatedObjectOnPointerMoved;
    AssociatedObject.Loaded -= AssociatedObjectOnLoaded;
  }

  private void AssociatedObjectOnLoaded(object? sender, RoutedEventArgs e)
  {
    if (AssociatedObject.DataContext is CanvasEntity model)
      _model = model;
    else
      throw new Exception("只能依附在数据上下文为CanvasEntity的元素上");
    if (AssociatedObject.Parent.Parent.DataContext is DragAndZoomViewModel dvm)
      dragAndZoomViewModel = dvm;
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
    var adjustedDelta = new Point(delta.X / dragAndZoomViewModel.Scale, delta.Y / dragAndZoomViewModel.Scale);
    
    // 增量叠加 每次移动10个单位
    var snappedDeltaX = Math.Round(adjustedDelta.X / 10) * 10;
    var snappedDeltaY = Math.Round(adjustedDelta.Y / 10) * 10;

    if (snappedDeltaX != 0 || snappedDeltaY != 0)
    {
      _model.Left += snappedDeltaX;
      _model.Top += snappedDeltaY;
      _lastPointerPosition = new Point(
        _lastPointerPosition.X + snappedDeltaX * dragAndZoomViewModel.Scale,
        _lastPointerPosition.Y + snappedDeltaY * dragAndZoomViewModel.Scale
      );
    }
  }

  private void AssociatedObjectOnPointerReleased(object? sender, PointerReleasedEventArgs e)
  {
    _isDragging = false;
    e.Pointer.Capture(null);
  }
}