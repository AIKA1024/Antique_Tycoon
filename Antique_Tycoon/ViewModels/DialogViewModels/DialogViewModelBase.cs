using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public abstract class DialogViewModelBase : ObservableObject
{
  public bool IsLightDismissEnabled { get; set; }
}