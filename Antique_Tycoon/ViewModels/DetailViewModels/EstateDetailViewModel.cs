using Antique_Tycoon.Models.Nodes;
using CommunityToolkit.Mvvm.Input;

namespace Antique_Tycoon.ViewModels.DetailViewModels;

public partial class EstateDetailViewModel(Estate model) : NodeDetailViewModel(model)
{
  public Estate Estate => (Estate)Model;

  [RelayCommand]
  private void AddLevel()
  {
    Estate.RevenueModifiers.Add(100);
  }

  [RelayCommand]
  private void RemoveLevel(int level)
  {
    
  }
}