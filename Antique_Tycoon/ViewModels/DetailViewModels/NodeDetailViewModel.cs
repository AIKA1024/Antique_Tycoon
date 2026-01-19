using Antique_Tycoon.Models.Nodes;

namespace Antique_Tycoon.ViewModels.DetailViewModels;

public class NodeDetailViewModel(NodeModel model)
{
  public NodeModel Model { get; } = model;
}