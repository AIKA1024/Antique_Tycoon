using Antique_Tycoon.Models;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class MapListPageViewModel : PageViewModelBase
{
  [ObservableProperty] public partial ObservableCollection<Map> Maps { get; set; }

  [RelayCommand]
  private async Task ShowCreateDialog()
  {
    await App.Current.Services.GetRequiredService<DialogService>().ShowDialogAsync(new CreateMapDialogViewModel { IsLightDismissEnabled = true });
  }

  public override void OnNavigatedTo()
  {
    base.OnNavigatedTo();
    Maps =  new ObservableCollection<Map>(App.Current.Services.GetRequiredService<MapFileService>().GetMaps());
  }
}