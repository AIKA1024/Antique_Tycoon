using System.Collections.Generic;
using Antique_Tycoon.Models.Node;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Antique_Tycoon.Messages;

public class GameMaskShowMessage(NodeModel[] selectableNodes):AsyncRequestMessage<string>
{
  public NodeModel[] SelectableNodes => selectableNodes;
}