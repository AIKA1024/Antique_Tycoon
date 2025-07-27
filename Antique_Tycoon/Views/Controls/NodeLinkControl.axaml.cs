using System;
using Antique_Tycoon.ViewModels;
using Antique_Tycoon.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Views.Controls;

public class NodeLinkControl : ContentControl
{
  private DragAndZoomViewModel _dragAndZoomViewModel;
  private Point _dragOffset;
  private bool _isDragging;
  private Point _lastPointerPosition;
  
  public static readonly RoutedEvent<RoutedEventArgs> MoveEvent =
    RoutedEvent.Register<NodeLinkControl, RoutedEventArgs>(nameof(Move), RoutingStrategies.Tunnel);

  public event EventHandler<RoutedEventArgs> Move
  { 
    add => AddHandler(MoveEvent, value);
    remove => RemoveHandler(MoveEvent, value);
  }

  // protected override void OnPointerPressed(PointerPressedEventArgs e)
  // {
  //   base.OnPointerPressed(e);
  //   if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
  //   {
  //     _isDragging = true;
  //     _lastPointerPosition = e.GetPosition(App.Current.Services.GetRequiredService<MainWindow>());
  //     e.Pointer.Capture(this);
  //   }
  // }
  //
  // protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
  // {
  //   base.OnApplyTemplate(e);
  //   if (Parent.Parent.DataContext is DragAndZoomViewModel dvm)
  //     _dragAndZoomViewModel = dvm;
  // }
  //
  // protected override void OnPointerMoved(PointerEventArgs e)
  // {
  //   base.OnPointerMoved(e);
  //   if (!_isDragging) return;
  //
  //   var currentPointerPosition = e.GetPosition(App.Current.Services.GetRequiredService<MainWindow>());
  //   var delta = currentPointerPosition - _lastPointerPosition;
  //   var adjustedDelta = new Point(delta.X / _dragAndZoomViewModel.Scale, delta.Y / _dragAndZoomViewModel.Scale);
  //   
  //   // 增量叠加 每次移动10个单位
  //   var snappedDeltaX = Math.Round(adjustedDelta.X / 10) * 10;
  //   var snappedDeltaY = Math.Round(adjustedDelta.Y / 10) * 10;
  //
  //   if (snappedDeltaX != 0 || snappedDeltaY != 0)
  //   {
  //     Canvas.SetLeft(Parent,snappedDeltaX);
  //     Canvas.SetTop(Parent,snappedDeltaY);
  //     // _model.Left += snappedDeltaX;
  //     // _model.Top += snappedDeltaY;
  //     _lastPointerPosition = new Point(
  //       _lastPointerPosition.X + snappedDeltaX * _dragAndZoomViewModel.Scale,
  //       _lastPointerPosition.Y + snappedDeltaY * _dragAndZoomViewModel.Scale
  //     );
  //   }
  // }
  //
  // protected override void OnPointerReleased(PointerReleasedEventArgs e)
  // {
  //   base.OnPointerReleased(e);
  //   _isDragging = false;
  //   e.Pointer.Capture(null);
  // }
}