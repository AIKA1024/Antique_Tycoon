using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.ViewModels;

public partial class MapEditPageViewModel:DragAndZoomViewModel
{
  [ObservableProperty] private bool _isShowFlyout;
}