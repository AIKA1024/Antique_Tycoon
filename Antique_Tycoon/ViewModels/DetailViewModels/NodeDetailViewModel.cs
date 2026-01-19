using Antique_Tycoon.Models.Nodes;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.ViewModels.DetailViewModels;

public class NodeDetailViewModel(NodeModel model) : ObservableObject
{
  public NodeModel Model { get; } = model;
}