using System;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Antique_Tycoon.Behaviors;

public static class DragEvents
{
  // 定义附加路由事件（冒泡策略）
  public static readonly RoutedEvent<RoutedEventArgs> DragEvent =
    RoutedEvent.Register<RoutedEventArgs>(
      "Drag",          // 事件名称
      RoutingStrategies.Direct,      // 路由策略（冒泡、隧道或直接）
      typeof(DragEvents));    // 所有者类型（通常是当前静态类）

  // 可选：提供 Add/Remove 方法简化订阅
  public static void AddDragHandler(Control element, EventHandler<RoutedEventArgs> handler)
  {
    element.AddHandler(DragEvent, handler);
  }
  public static void RemoveDragHandler(Control element, EventHandler<RoutedEventArgs> handler)
  {
    element.RemoveHandler(DragEvent, handler);
  }
  public static void RaiseDrag(Interactive source)
  {
    source.RaiseEvent(new RoutedEventArgs(DragEvent));
  }
}