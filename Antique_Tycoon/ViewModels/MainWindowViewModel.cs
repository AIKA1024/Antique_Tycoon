using Antique_Tycoon.Messages;
using Antique_Tycoon.ViewModels.PageViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Antique_Tycoon.ViewModels;

public partial class MainWindowViewModel : PageViewModelBase
{
  [ObservableProperty] private PageViewModelBase _currentPageViewModel = new PageViewModels.StartPageViewModel();

  [RelayCommand]
  private void KeyPressed(Avalonia.Input.KeyGesture key)
  {
    WeakReferenceMessenger.Default.Send(new KeyPressedMessage(key));
  }
}