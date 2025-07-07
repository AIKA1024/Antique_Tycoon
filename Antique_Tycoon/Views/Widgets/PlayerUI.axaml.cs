using Antique_Tycoon.Messages;
using Antique_Tycoon.ViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace Antique_Tycoon.Views.Widgets;

public partial class PlayerUI : UserControl
{
  public PlayerUI()
  {
    InitializeComponent();
    var playerUIViewModel = new PlayerUIViewModel();
    DataContext = playerUIViewModel;
    WeakReferenceMessenger.Default.Register<KeyPressedMessage>(this, (_, m) =>
    {
      if (m.Value.Key == Key.Tab)
        playerUIViewModel.IsVisible = !playerUIViewModel.IsVisible;
    });
  }
}