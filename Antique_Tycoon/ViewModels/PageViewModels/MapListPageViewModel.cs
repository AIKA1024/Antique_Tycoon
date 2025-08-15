using Antique_Tycoon.Models;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class MapListPageViewModel : PageViewModelBase
{
  public ObservableCollection<Map> Maps { get; set; } = new(App.Current.Services.GetRequiredService<MapFileService>().GetMaps());

  [RelayCommand]
  private async Task ShowCreateDialog()
  {
    await App.Current.Services.GetRequiredService<DialogService>().ShowDialogAsync(new CreateMapDialogViewModel { IsLightDismissEnabled = true });
  }
}