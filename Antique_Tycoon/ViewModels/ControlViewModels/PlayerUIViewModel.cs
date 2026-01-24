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
using Antique_Tycoon.Models.Nodes;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Antique_Tycoon.ViewModels.PageViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.ControlViewModels;

public partial class PlayerUiViewModel : PageViewModelBase,IDisposable
{
  private readonly GameManager _gameManager = App.Current.Services.GetRequiredService<GameManager>();

  private readonly ActionQueueService _actionQueueService =
    App.Current.Services.GetRequiredService<ActionQueueService>();

  [ObservableProperty] private bool _isVisible;
  [ObservableProperty] private Player _localPlayer;
  [ObservableProperty] private string _reminderText = "轮到你啦";
  [ObservableProperty] private bool _isShowReminderText;
  private string _rollDiceActionId = "";

  public DialogViewModelBase? DialogViewModel => _dialogService.CurrentDialogViewModel;

  [ObservableProperty] public partial bool RollButtonEnable { get; set; } = false;

  //PlayerUI自己的dialog服务，以实现按tab透明功能
  private readonly DialogService _dialogService = new();

  public ObservableCollection<Player> OtherPlayers { get; } = [];

  public PlayerUiViewModel()
  {
    WeakReferenceMessenger.Default.Register<RollDiceAction>(this, ReceiveRollDiceAction);
    WeakReferenceMessenger.Default.Register<TurnStartResponse>(this, ReceiveTurnStartMessage);
    WeakReferenceMessenger.Default.Register<AntiqueChanceResponse>(this, ReceiveAntiqueChanceResponse);
    WeakReferenceMessenger.Default.Register<GetAntiqueResultResponse>(this, ReceiveGetAntiqueResultResponse);
    WeakReferenceMessenger.Default.Register<SaleAntiqueAction>(this, ReceiveSaleAntiqueAction);
    _dialogService.DialogCollectionChanged += NotifyDialogViewModelChanged;
  }

  private void NotifyDialogViewModelChanged()
  {
    OnPropertyChanged(nameof(DialogViewModel));
  }

  public void Dispose()
  {
    _dialogService.DialogCollectionChanged -= NotifyDialogViewModelChanged;
  }
  
  private async void ReceiveSaleAntiqueAction(object recipient, SaleAntiqueAction message)
  {
    _actionQueueService.Enqueue(async () =>
    {
      var antiqueMapItems = _gameManager.LocalPlayer.Antiques.GroupBy(a => a.Index)
        .Select(group => new AntiqueStack(group.First(), group.Count())).ToArray();
      var saleAntiqueDialogViewModel = new SaleAntiqueDialogViewModel(antiqueMapItems)
        { IsLightDismissEnabled = false };
      var saleAntiqueDetermination = await _dialogService.ShowDialogAsync(saleAntiqueDialogViewModel);
    });
  }

  private void ReceiveGetAntiqueResultResponse(object recipient, GetAntiqueResultResponse message)
  {
    if (message.IsSuccess)
    {
      _actionQueueService.Enqueue(() =>
      {
        ReminderText = $"{_gameManager.GetPlayerByUuid(message.PlayerUuid).Name}成功获得该古玩";
        return Task.Delay(1000);
      });
    }
    else
    {
      _actionQueueService.Enqueue(() =>
      {
        ReminderText = $"{_gameManager.GetPlayerByUuid(message.PlayerUuid).Name}未能获得古玩，流入市场";
        return Task.Delay(1000);
      });
    }
  }

  private void ReceiveAntiqueChanceResponse(object recipient, AntiqueChanceResponse message)
  {
    _actionQueueService.Enqueue(() =>
    {
      ReminderText = "再次投骰子以获取古玩";
      return Task.CompletedTask;
    });
  }

  private void ReceiveRollDiceAction(object recipient, RollDiceAction message)
  {
    _actionQueueService.Enqueue(() =>
    {
      RollButtonEnable = true;
      _rollDiceActionId = message.Id;
      return Task.CompletedTask;
    });
  }

  private void ReceiveTurnStartMessage(object sender, TurnStartResponse message)
  {
    IsShowReminderText = false;
    if (message.PlayerUuid == _gameManager.LocalPlayer.Uuid)
    {
      ReminderText = "轮到你啦";
      IsShowReminderText = true;
    }
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