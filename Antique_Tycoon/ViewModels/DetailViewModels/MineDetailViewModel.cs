using Antique_Tycoon.Models.Nodes;

namespace Antique_Tycoon.ViewModels.DetailViewModels;

public class MineDetailViewModel(Mine model) : NodeDetailViewModel(model)
{
  public Mine Mine => (Mine)Model;
}