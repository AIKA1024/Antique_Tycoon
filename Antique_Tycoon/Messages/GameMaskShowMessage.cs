using System.Collections.Generic;
using System.Linq;
using Antique_Tycoon.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;
using NodeModel = Antique_Tycoon.Models.Nodes.NodeModel;

namespace Antique_Tycoon.Messages;

public class GameMaskShowMessage:AsyncRequestMessage<string>
{
  public GameMaskShowMessage(List<NodeModel> selectableNodes)
  {
    SelectableNodes = selectableNodes;
  }

  public GameMaskShowMessage(List<string> selectableNodesUuid,Map map)
  {
    List<NodeModel> tempList = [];
    tempList.AddRange(selectableNodesUuid.Select(uuid => (NodeModel)map.EntitiesDict[uuid]));
    SelectableNodes = tempList;
  }
  public List<NodeModel> SelectableNodes;
}