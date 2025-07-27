using Antique_Tycoon.ViewModels.PageViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.ViewModels.ControlViewModels;

public partial class PlayerUIViewModel:PageViewModelBase
{
  [ObservableProperty] private bool _isVisible;
}

