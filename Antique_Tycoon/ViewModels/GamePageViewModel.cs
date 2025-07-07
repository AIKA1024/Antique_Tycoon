using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.ViewModels;

public partial class GamePageViewModel : ViewModelBase
{
  [ObservableProperty] private double _scale = 1.0;
  [ObservableProperty] private Point _offset;
}