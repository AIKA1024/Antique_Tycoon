using Antique_Tycoon.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class GamePageViewModel:PageViewModelBase
{
  [ObservableProperty] private Map _map;
}