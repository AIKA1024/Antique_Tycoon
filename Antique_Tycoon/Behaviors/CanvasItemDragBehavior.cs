using System;
using System.Collections.Generic;
using Antique_Tycoon.Models.Node;
using Antique_Tycoon.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Xaml.Interactivity;
using Microsoft.Extensions.DependencyInjection;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Behaviors;

public partial class CanvasItemDragBehavior : Behavior<Control>
{
  private bool _isDragging;
  private Point _lastPointerPosition;
  private IDisposable? _topSubscription;
  private IDisposable? _leftSubscription;
  public double DragThreshold { get; set; } = 4;
  [GeneratedDirectProperty] public partial IList<CanvasItemModel>? SelectedItems { get; set; }
  [GeneratedDirectProperty] public partial double Scale { get; set; }

  [GeneratedDirectProperty] public partial Point Offset { get; set; }

  protected override void OnAttached()
  {
    base.OnAttached();
    AssociatedObject.PointerPressed += AssociatedObjectOnPointerPressed;
    AssociatedObject.PointerReleased += AssociatedObjectOnPointerReleased;
    AssociatedObject.PointerMoved += AssociatedObjectOnPointerMoved;
    AssociatedObject.SizeChanged += AssociatedObjectOnSizeChanged;
    _topSubscription = AssociatedObject.GetObservable(Canvas.TopProperty)
      .Subscribe(_ => LayoutChanged.RaiseLayoutChanged(AssociatedObject));
    _leftSubscription = AssociatedObject.GetObservable(Canvas.LeftProperty)
      .Subscribe(_ => LayoutChanged.RaiseLayoutChanged(AssociatedObject));
  }

  private void AssociatedObjectOnSizeChanged(object? sender, SizeChangedEventArgs e) =>
    LayoutChanged.RaiseLayoutChanged(AssociatedObject);

  protected override void OnDetaching()
  {
    base.OnDetaching();
    AssociatedObject.PointerPressed -= AssociatedObjectOnPointerPressed;
    AssociatedObject.PointerReleased -= AssociatedObjectOnPointerReleased;
    AssociatedObject.PointerMoved -= AssociatedObjectOnPointerMoved;
    AssociatedObject.SizeChanged -= AssociatedObjectOnSizeChanged;
    _topSubscription?.Dispose();
    _leftSubscription?.Dispose();
  }

    

  private void AssociatedObjectOnPointerPressed(object? sender, PointerPressedEventArgs e)
  {
    if (!e.GetCurrentPoint(AssociatedObject).Properties.IsLeftButtonPressed || !IsEnabled) return;
    _isDragging = true;
    _lastPointerPosition = e.GetPosition(App.Current.Services.GetRequiredService<MainWindow>());
    e.Pointer.Capture(AssociatedObject);
  }

  private void AssociatedObjectOnPointerMoved(object? sender, PointerEventArgs e)
  {
    if (!_isDragging || !IsEnabled) return;

    var currentPointerPosition = e.GetPosition(App.Current.Services.GetRequiredService<MainWindow>());
    var delta = currentPointerPosition - _lastPointerPosition;

    if (!_isDragging)
    {
      if (Math.Abs(delta.X) > DragThreshold || Math.Abs(delta.Y) > DragThreshold)
      {
        _isDragging = true; // 正式进入拖拽模式
        _lastPointerPosition = currentPointerPosition; // 初始化拖拽起点
      }
      else
      {
        return; // 阈值没到，直接退出
      }
    }


    var adjustedDelta = new Point(delta.X / Scale, delta.Y / Scale);
    // 增量叠加 每次移动10个单位
    var snappedDeltaX = Math.Round(adjustedDelta.X / 10) * 10;
    var snappedDeltaY = Math.Round(adjustedDelta.Y / 10) * 10;

    if (snappedDeltaX != 0 || snappedDeltaY != 0)
    {
      foreach (var item in SelectedItems)
      {
        if (item is NodeModel nodeModel)
        {
          nodeModel.Left += snappedDeltaX;
          nodeModel.Top += snappedDeltaY;
        }
      }

      _lastPointerPosition = new Point(
        _lastPointerPosition.X + snappedDeltaX * Scale,
        _lastPointerPosition.Y + snappedDeltaY * Scale
      );
    }
  }

  private void AssociatedObjectOnPointerReleased(object? sender, PointerReleasedEventArgs e)
  {
    if (!IsEnabled) return;
    _isDragging = false;
    e.Pointer.Capture(null);
  }
}