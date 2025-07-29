using Antique_Tycoon.Models;
using Antique_Tycoon.Services;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class MapListPageViewModel : PageViewModelBase
{
  public AvaloniaList<Map> Maps { get; set; } = [];

  [RelayCommand]
  private void NavigateToMapEditPage()
  {
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new MapEditPageViewModel());
  }
}