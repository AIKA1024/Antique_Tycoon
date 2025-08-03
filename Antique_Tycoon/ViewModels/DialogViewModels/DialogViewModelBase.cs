using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public abstract partial class DialogViewModelBase : ObservableValidator
{
  public bool IsLightDismissEnabled { get; set; }
  [RelayCommand]
  protected void CloseDialog()
  {
    App.Current.Services.GetRequiredService<DialogService>().CloseDialog(this);
  }
}