using System.Threading.Tasks;
using Antique_Tycoon.Models;
using Antique_Tycoon.Net;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class StartPageViewModel : PageViewModelBase
{
  public Player LocalPlayer { get; } = App.Current.Services.GetRequiredService<GameManager>().LocalPlayer;

  [RelayCommand]
  private void NavigateToHallPage()
  {
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new HallPageViewModel(
      App.Current.Services.GetRequiredService<NetClient>(), App.Current.Services.GetRequiredService<GameManager>()));
  }

  [RelayCommand]
  private void NavigateToMapListPage()
  {
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new MapListPageViewModel());
  }

  [RelayCommand]
  private async Task NavigateToSettingPage()
  {
    var commonDialogViewModel = new MessageDialogViewModel
    {
      Title = "提示",
      Message = "还没实现"
    };
    await App.Current.Services.GetRequiredService<DialogService>().ShowDialogAsync(commonDialogViewModel);
  }
}