using System;
using System.Linq;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Connections;
using Antique_Tycoon.Models.Node;
using Antique_Tycoon.ViewModels;
using Antique_Tycoon.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Views.Controls;

public partial class NodeLinkControl : ContentControl
{
  [GeneratedDirectProperty] public partial CanvasEntity CanvasEntity { get; set; }
  [GeneratedDirectProperty] public partial Map Map { get; set; }
  [GeneratedDirectProperty] public partial Panel LineCanvas { get; set; }

  public NodeLinkControl()
  {
    AddHandler(Connector.ConnectedEvent, OnConnectorConnected, RoutingStrategies.Bubble);
  }

  protected override void OnLoaded(RoutedEventArgs e)
  {
    base.OnLoaded(e);

    ConnectLine();
  }

  private void ConnectLine()
  {
    var mapEntitiesDic = Map.Entities.ToDictionary(e => e.Uuid, e => e);
    foreach (var connectionModel in CanvasEntity.ConnectionModels)
    {
      var startConnectorModel = CanvasEntity.ConnectorModels.First(c => c.Uuid == connectionModel.StartConnectorId);
      var endEntity = mapEntitiesDic[connectionModel.EndNodeId];
      var endConnectorModel = endEntity.ConnectorModels.First(c => c.Uuid == connectionModel.EndConnectorId);
      var connection = new Connection(startConnectorModel, endConnectorModel);
      startConnectorModel.ActiveConnections.Add(connection);
      endConnectorModel.PassiveConnections.Add(connection);
      LineCanvas.Children.Add(connection.Line);
    }
  }

  private void OnConnectorConnected(object? sender, Connector.ConnectedRoutedEventArgs e)
  {
    CanvasEntity.ConnectionModels.Add(e.ConnectionModel);
    e.Handled = true;
  }
}