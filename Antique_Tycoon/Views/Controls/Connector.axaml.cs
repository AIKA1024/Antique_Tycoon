using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Antique_Tycoon.Behaviors;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Connections;
using Antique_Tycoon.Models.Node;
using Antique_Tycoon.ViewModels.ControlViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Views.Controls;

public partial class Connector : TemplatedControl
{
  private const int CheckInterval = 100;
  private ConnectionLine? _tempLine;
  private Point _lastCheckedPosition;
  private DateTime _lastCheckTime;
  private Connector? _closestConnector;
  public ConnectorModel SelfConnectorModel;
  
  [GeneratedDirectProperty] public partial List<Connection> ActiveConnections { get; set; } = [];
  [GeneratedDirectProperty] public partial List<Connection> PassiveConnections { get; set; } = [];

  [GeneratedStyledProperty] public partial IBrush? Fill { get; set; }
  [GeneratedStyledProperty] public partial IBrush? Stroke { get; set; }
  [GeneratedStyledProperty(2)] public partial double StrokeThickness { get; set; }

  [GeneratedDirectProperty] public partial Panel LineCanvas { get; set; }

  [GeneratedDirectProperty] public partial Point Anchor { get; set; }
  [GeneratedDirectProperty] public partial string Uuid { get; set; } = "";
  [GeneratedDirectProperty] public partial ICommand? Command { get; set; }
  [GeneratedDirectProperty] public partial object? CommandParameter { get; set; }

  public static readonly RoutedEvent<ConnectedRoutedEventArgs> ConnectedEvent =
    RoutedEvent.Register<Connector, ConnectedRoutedEventArgs>(nameof(Connected), RoutingStrategies.Bubble);

  public event EventHandler<ConnectedRoutedEventArgs> Connected
  {
    add => AddHandler(ConnectedEvent, value);
    remove => RemoveHandler(ConnectedEvent, value);
  }

  public class ConnectedRoutedEventArgs(ConnectionModel connectionModel) : RoutedEventArgs
  {
    public ConnectionModel ConnectionModel { get; set; } = connectionModel;
  }

  protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
  {
    base.OnApplyTemplate(e);
    if (LineCanvas == null)
      throw new NullReferenceException("LineCanvas");
    LineCanvas.PointerMoved += Canvas_PointerMoved;
    LineCanvas.PointerReleased += Canvas_PointerReleased;
    SelfConnectorModel = DataContext as ConnectorModel;
  }

  protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
  {
    base.OnAttachedToVisualTree(e);
    var parent = this.GetVisualAncestors().OfType<NodeLinkControl>().FirstOrDefault();
    if (parent != null)
      LayoutChanged.AddLayoutChangedHandler(parent, OnNoteLocationChanged);
  }

  private void OnNoteLocationChanged(object? sender, RoutedEventArgs e)
  {
    Dispatcher.UIThread.Post(() =>
    {
      UpdateAnchor();
      foreach (var activeConnection in ActiveConnections)
        activeConnection.Update();

      foreach (var passiveConnection in PassiveConnections)
        passiveConnection.Update();
    }, DispatcherPriority.Render);
  }

  public void UpdateAnchor()
  {
    var position = this.TranslatePoint(new Point(Width / 2, Height / 2), LineCanvas) ?? default;
    Anchor = position;
  }

  public Connector? FindClosestConnector(Point worldPos)
  {
    Connector? closest = null;
    double minDist = double.MaxValue;

    foreach (var connector in LineCanvas.GetVisualDescendants().OfType<Connector>())
    {
      connector.UpdateAnchor();
      if (connector == this || connector.Parent == Parent)
        continue;
      var pos = connector.Anchor; // 世界坐标，需确保 Anchor 正确更新
      var dist = ((Vector)(pos - worldPos)).Length;

      if (dist < minDist && dist < Width / 2)
      {
        minDist = dist;
        closest = connector;
      }
    }

    return closest;
  }


  protected override void OnPointerPressed(PointerPressedEventArgs e)
  {
    base.OnPointerPressed(e);
    if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
      CancelConnect();
    else
    {
      var position = this.TranslatePoint(new Point(Width / 2, Width / 2), LineCanvas) ?? new Point(0, 0);
      _tempLine = null;
      _tempLine = new ConnectionLine
      {
        StartPoint = position,
        EndPoint = position,
        StrokeDashArray = [6, 4],
        Stroke = Stroke,
        StrokeThickness = StrokeThickness
      };
      LineCanvas.Children.Add(_tempLine);
    }

    e.Handled = true;
  }

  private void Canvas_PointerMoved(object? sender, PointerEventArgs e)
  {
    var isLeftPressed = e.GetCurrentPoint(sender as Visual).Properties.IsLeftButtonPressed;

    if (_tempLine == null || !isLeftPressed) return;

    var pos = e.GetPosition(LineCanvas);
    if ((DateTime.Now - _lastCheckTime).TotalMilliseconds > CheckInterval ||
        ((Vector)(_lastCheckedPosition - pos)).Length > 8)
    {
      _lastCheckTime = DateTime.Now;
      _lastCheckedPosition = pos;
      _closestConnector = FindClosestConnector(pos);
      if (_closestConnector != null && _closestConnector != this)
        _tempLine.EndPoint = _closestConnector?.Anchor ?? e.GetPosition(LineCanvas);
    }

    if (_closestConnector is null)
      _tempLine.EndPoint = e.GetPosition(LineCanvas);
    _tempLine.UpdateGeometry();
    e.Handled = true;
  }

  private void Canvas_PointerReleased(object? sender, PointerReleasedEventArgs e)
  {
    if (_tempLine == null) return;
    LineCanvas.Children.Remove(_tempLine);
    _tempLine = null;

    if (_closestConnector == null) return;
    var connection = new Connection(SelfConnectorModel, _closestConnector.SelfConnectorModel);
    ActiveConnections.Add(connection);
    _closestConnector.PassiveConnections.Add(connection);
    LineCanvas.Children.Add(connection.Line);
    if (Command?.CanExecute(CommandParameter) == true)
      Command.Execute(CommandParameter);
    RaiseEvent(new ConnectedRoutedEventArgs(new ConnectionModel
    {
      StartConnectorId = Uuid,
      EndConnectorId = _closestConnector.Uuid,
      EndNodeId = ((CanvasEntity)_closestConnector.Parent.DataContext).Uuid
    }) { RoutedEvent = ConnectedEvent, Source = this });
    e.Handled = true;
  }

  private void CancelConnect()
  {
    foreach (var activeConnection in ActiveConnections)
      LineCanvas.Children.Remove(activeConnection.Line);
    foreach (var passiveConnection in PassiveConnections)
      LineCanvas.Children.Remove(passiveConnection.Line);
    ActiveConnections.Clear();
    PassiveConnections.Clear();
  }
}