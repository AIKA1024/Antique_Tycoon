using Antique_Tycoon.Models.Nodes;

namespace Antique_Tycoon.ViewModels.DetailViewModels;

public class EstateDetailViewModel(Estate model) : NodeDetailViewModel(model)
{
  public Estate EstateModel => (Estate)Model;
}