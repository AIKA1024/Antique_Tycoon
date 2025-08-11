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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Antique_Tycoon.Views.Controls;

public partial class NodeLinkControl : ContentControl
{
  [GeneratedDirectProperty] public partial NodeModel NodeModel { get; set; }
  [GeneratedDirectProperty] public partial Map Map { get; set; }
  [GeneratedDirectProperty] public partial Panel LineCanvas { get; set; }

  public NodeLinkControl()
  {
    //AddHandler(Connector.ConnectedEvent, OnConnectorConnected, RoutingStrategies.Bubble);
    //AddHandler(Connector.CancelConnectEvent, OnCancelConnectorConnected, RoutingStrategies.Bubble);
  }

  protected override void OnLoaded(RoutedEventArgs e)
  {
    base.OnLoaded(e);
  }

  

  //private void OnConnectorConnected(object? sender, Connector.ConnectedRoutedEventArgs e)
  //{
  //  NodeModel.ConnectionModels.Add(e.Connection);
  //  e.Handled = true;
  //}

  //private void OnCancelConnectorConnected(object? sender, Connector.CancelConnectRoutedEventArgs e)
  //{
  //  var connectionList = new List<Connection>();
  //  for (int i = NodeModel.ConnectionModels.Count - 1; i >= 0; i--)
  //  {
  //    if (NodeModel.ConnectionModels[i].Uuid == e.ConnectorUuid)
  //      NodeModel.ConnectionModels.RemoveAt(i);
  //  }
  //  e.Handled = true;
  //}
}