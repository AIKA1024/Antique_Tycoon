using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels;

public partial class HallPageViewModel:ViewModelBase
{
  [RelayCommand]
  private void NavigateToCreateRoomPage()
  {
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new CreateRoomPageViewModel());
  }
}