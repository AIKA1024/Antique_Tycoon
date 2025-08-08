using System;
using Antique_Tycoon.Models.Node;
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
  private CanvasEntity _canvasEntity;
  public NodeLinkControl()
  {
    AddHandler(Connector.ConnectedEvent,OnConnectorConnected,RoutingStrategies.Bubble);
  }

  protected override void OnLoaded(RoutedEventArgs e)
  {
    base.OnLoaded(e);
    _canvasEntity = DataContext as CanvasEntity;
  }

  private void OnConnectorConnected(object? sender, Connector.ConnectedRoutedEventArgs e)
  {
    _canvasEntity.ConnectionModels.Add(e.ConnectionModel);
    e.Handled = true;
  }
}