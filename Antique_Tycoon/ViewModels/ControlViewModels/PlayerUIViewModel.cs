using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.PageViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.ControlViewModels;

public partial class PlayerUiViewModel:PageViewModelBase
{
  [ObservableProperty] private bool _isVisible;
  [ObservableProperty] private Player _localPlayer;
  [ObservableProperty] private bool _rollButtonEnable;
  public ObservableCollection<Player> OtherPlayers { get; } = [];

  public PlayerUiViewModel()
  {
    WeakReferenceMessenger.Default.Register<TurnStartMessage>(this, ReceiveTurnStartMessage);
  }
  
  private void ReceiveTurnStartMessage(object recipient, TurnStartMessage message)//todo 可能需要定期问服务器到自己没，因为网络不可靠
  {
    if (message.Value == LocalPlayer.Uuid)
      RollButtonEnable = true;
  }
  
  [RelayCommand]
  private async Task RollDiceAsync()
  {
    await App.Current.Services.GetRequiredService<GameManager>().RollDiceAsync();
    await Task.Delay(1000);
  }
}

