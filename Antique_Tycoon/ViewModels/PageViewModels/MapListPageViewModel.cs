using System.Threading.Tasks;
using Antique_Tycoon.Models;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class MapListPageViewModel : PageViewModelBase
{
  public AvaloniaList<Map> Maps { get; set; } = [];

  [RelayCommand]
  private async Task ShowCreateDialog()
  {
     await App.Current.Services.GetRequiredService<DialogService>().ShowDialogAsync(new CreateMapDialogViewModel { IsLightDismissEnabled = true });
  }
}