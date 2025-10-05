using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Tmds.DBus.Protocol;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public abstract partial class DialogViewModelBase : ObservableValidator
{
  [ObservableProperty] public partial double MaxWidthPercent { get; set; } = .8f;  // 0~1
  [ObservableProperty] public partial double MaxHeightPercent { get; set; } = .8f; // 0~1
  public bool IsLightDismissEnabled { get; set; } = true;
  public void CloseDialog()
  {
    App.Current.Services.GetRequiredService<DialogService>().CloseDialog(this);
  }
}

public abstract class DialogViewModelBase<T> : DialogViewModelBase
{
  public void CloseDialog(T result)
  {
    App.Current.Services.GetRequiredService<DialogService>().CloseDialog(this, result);
  }
}