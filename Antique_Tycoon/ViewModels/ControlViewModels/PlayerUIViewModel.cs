using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Entities;
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
  private readonly GameManager _gameManger = App.Current.Services.GetRequiredService<GameManager>();

  [ObservableProperty] private bool _isVisible;
  [ObservableProperty] private Player _localPlayer;
  [ObservableProperty] private Antique _antique;//todo 这个要搞一个父类，表示要展示的卡片的基本信息
  private string _rollDiceActionId = "";

  [ObservableProperty] public partial bool RollButtonEnable { get; set; } = false;

  public ObservableCollection<Player> OtherPlayers { get; } = [];

  public PlayerUiViewModel()
  {
    WeakReferenceMessenger.Default.Register<TurnStartResponse>(this, ReceiveTurnStartMessage);
    WeakReferenceMessenger.Default.Register<RollDiceAction>(this,ReceiveRollDiceAction);
    // WeakReferenceMessenger.Default.Register<AntiqueChanceResponse>(this,ReceiveAntiqueChanceResponse);
  }

  // private void ReceiveAntiqueChanceResponse(object recipient, AntiqueChanceResponse message)
  // {
  //   Antique = _gameManger.SelectedMap.Antiques.First(a => a.Uuid == message.AntiqueUuid);
  // }

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
    Debug.WriteLine("发起投骰子请求");
    RollButtonEnable = false;
    await App.Current.Services.GetRequiredService<GameRuleService>().RollDiceAsync(_rollDiceActionId);
    Debug.WriteLine("请求结束");
  }
}