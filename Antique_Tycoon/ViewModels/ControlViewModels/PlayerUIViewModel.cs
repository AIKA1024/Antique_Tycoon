using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Net.Tcp.Response.GameAction;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.PageViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.ControlViewModels;

public partial class PlayerUiViewModel : PageViewModelBase
{
  private readonly AnimationManager _animationManager = App.Current.Services.GetRequiredService<AnimationManager>();

  [ObservableProperty] private bool _isVisible;
  [ObservableProperty] private Player _localPlayer;
  private string _rollDiceActionId = "";

  [ObservableProperty] public partial bool RollButtonEnable { get; set; } = false;

  public ObservableCollection<Player> OtherPlayers { get; } = [];

  public PlayerUiViewModel()
  {
    WeakReferenceMessenger.Default.Register<TurnStartResponse>(this, ReceiveTurnStartMessage);
    WeakReferenceMessenger.Default.Register<RollDiceAction>(this,ReceiveRollDiceAction);
  }

  private void ReceiveRollDiceAction(object recipient, RollDiceAction message)
  {
    RollButtonEnable = true;
    _rollDiceActionId = message.Id;
  }

  private void ReceiveTurnStartMessage(object recipient, TurnStartResponse message)
  {
    // if (message.PlayerUuid == LocalPlayer.Uuid)
      // RollButtonEnable = true;
  }

  [RelayCommand]
  private async Task RollDiceAsync()
  {
    if (_animationManager.HasAnimationRunning)
      return;
    RollButtonEnable = false;
    await App.Current.Services.GetRequiredService<GameRuleService>().RollDiceAsync(_rollDiceActionId);
  }
}