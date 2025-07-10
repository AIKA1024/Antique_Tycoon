using Antique_Tycoon.Models;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels;

public partial class StartPageViewModel:ViewModelBase
{
  public Player SelfPlayer { get; } = App.Current.Services.GetRequiredService<Player>();
  
  [RelayCommand]
  private void NavigateToGamePage()
  {
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new HallPageViewModel());
  }
}