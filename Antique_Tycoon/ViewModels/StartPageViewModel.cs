using Antique_Tycoon.Services;
using Antique_Tycoon.Views.Pages;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels;

public partial class StartPageViewModel:ViewModelBase
{
  [RelayCommand]
  private void NavigateToGamePage()
  {
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new HallPage());
  }
}