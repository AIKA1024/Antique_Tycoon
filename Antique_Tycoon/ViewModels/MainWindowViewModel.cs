using Antique_Tycoon.Messages;
using Antique_Tycoon.Views.Pages;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Antique_Tycoon.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
  [ObservableProperty] private UserControl _currentPage = new StartPage();

  [RelayCommand]
  private void KeyPressed(Avalonia.Input.KeyGesture key)
  {
    WeakReferenceMessenger.Default.Send(new KeyPressedMessage(key));
  }
}