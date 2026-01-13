using System.Linq;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Connections;
using Antique_Tycoon.Views.Controls;
using NodeModel = Antique_Tycoon.Models.Nodes.NodeModel;

namespace Antique_Tycoon.Extensions;

public static class ConnectorExtension
{
  public static void CancelConnects(this Connector connector,Map map)
  {
    foreach (var activeConnection in connector.ActiveConnections)
    {
      if (map.EntitiesDict[activeConnection.EndNodeId] is NodeModel nodeModel)
        nodeModel.ConnectorModels.First(c => c.Uuid == activeConnection.EndConnectorId).PassiveConnections
          .Remove(activeConnection);
      map.Entities.Remove(activeConnection);
    }
    foreach (var passiveConnection in connector.PassiveConnections)
    {
      if (map.EntitiesDict[passiveConnection.StartNodeId] is NodeModel nodeModel)
        nodeModel.ConnectorModels.First(c => c.Uuid == passiveConnection.StartConnectorId).ActiveConnections
          .Remove(passiveConnection);
      map.Entities.Remove(passiveConnection);
    }
  }
  
  public static void CancelConnects(this ConnectorJsonModel model,Map map)
  {
    foreach (var activeConnection in model.ActiveConnections)
    {
      if (map.EntitiesDict[activeConnection.EndNodeId] is NodeModel nodeModel)
        nodeModel.ConnectorModels.First(c => c.Uuid == activeConnection.EndConnectorId).PassiveConnections
          .Remove(activeConnection);
      map.Entities.Remove(activeConnection);
    }
    foreach (var passiveConnection in model.PassiveConnections)
    {
      if (map.EntitiesDict[passiveConnection.StartNodeId] is NodeModel nodeModel)
        nodeModel.ConnectorModels.First(c => c.Uuid == passiveConnection.StartConnectorId).ActiveConnections
          .Remove(passiveConnection);
      map.Entities.Remove(passiveConnection);
    }
  }
}