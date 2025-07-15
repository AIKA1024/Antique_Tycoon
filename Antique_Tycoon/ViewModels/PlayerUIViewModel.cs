using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Antique_Tycoon.ViewModels;

public partial class PlayerUIViewModel:PageViewModelBase
{
  [ObservableProperty] private bool _isVisible;
}

