using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public partial class MessageDialogViewModel : DialogViewModelBase<bool?>
{
  [ObservableProperty] public partial string Title { get; set; } = "";
  [ObservableProperty] public partial string Message { get; set; } = "";
  [ObservableProperty] public partial bool IsShowCancelButton { get; set; }
  [ObservableProperty] public partial bool IsShowConfirmButton { get; set; } = true;

  [RelayCommand]
  private void Confirm()
  {
    CloseDialog(true);
    // App.Current.Services.GetRequiredService<DialogService>().CloseDialog(this,true);
  }
  
  [RelayCommand]
  private void Cancel()
  {
    CloseDialog(false);
    // App.Current.Services.GetRequiredService<DialogService>().CloseDialog(this,false);
  }
}