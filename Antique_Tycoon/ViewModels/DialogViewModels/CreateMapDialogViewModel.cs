using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public partial class CreateMapDialogViewModel : DialogViewModelBase
{
  [ObservableProperty] private string _mapName = "";

  [RelayCommand]
  private void CloseDialog()
  {
    App.Current.Services.GetRequiredService<DialogService>().CloseDialog(this);
  }
}