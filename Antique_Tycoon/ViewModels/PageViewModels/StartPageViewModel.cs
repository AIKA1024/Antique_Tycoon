using Antique_Tycoon.Models;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class StartPageViewModel:PageViewModelBase
{
  public Player SelfPlayer { get; } = App.Current.Services.GetRequiredService<Player>();
  
  [RelayCommand]
  private void NavigateToHallPage()
  {
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new PageViewModels.HallPageViewModel());
  }
  [RelayCommand]
  private void NavigateToMapEditPage()
  {
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new PageViewModels.MapEditPageViewModel());
  }
}