using System.Linq;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Net.Tcp.Response.GameAction;
using Antique_Tycoon.Models.Node;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class GamePageViewModel : PageViewModelBase
{
    [ObservableProperty] private Map _map;
    private readonly GameManager _gameManager = App.Current.Services.GetRequiredService<GameManager>();
    // private readonly GameRuleService _gameRuleService = App.Current.Services.GetRequiredService<GameRuleService>();
    private readonly DialogService _dialogService = App.Current.Services.GetRequiredService<DialogService>();
    private readonly AnimationManager _animationManager = App.Current.Services.GetRequiredService<AnimationManager>();

    [ObservableProperty] private string _reminderText = "轮到你啦";
    [ObservableProperty] private int _rollDiceValue;
    [ObservableProperty] private bool _isShowReminderText;


    /// <summary>
    /// 是否在高亮给玩家选择卡片阶段
    /// </summary>
    private bool _isHighlightMode;

    public GamePageViewModel(Map map)
    {
        Map = map;
        foreach (var player in _gameManager.Players)
            Map.SpawnNode.PlayersHere.Add(player);

        // WeakReferenceMessenger.Default.Register<NodeClickedMessage>(this, ReceiveNodeClicked);
        WeakReferenceMessenger.Default.Register<TurnStartResponse>(this, ReceiveTurnStartMessage);
        WeakReferenceMessenger.Default.Register<InitGameResponse>(this, ReceiveInitGameMessage);
        // WeakReferenceMessenger.Default.Register<RollDiceResponse>(this, ReceiveRollDiceMessage);
        WeakReferenceMessenger.Default.Register<UpdateEstateInfoResponse>(this, ReceiveUpdateEstateInfoMessage);
        WeakReferenceMessenger.Default.Register<BuyEstateAction>(this ,ReceiveBuyEstateAction);
    }

    private async void ReceiveBuyEstateAction(object recipient, BuyEstateAction message)
    {
        await _animationManager.WaitAnimation(message.WaitAnimationToken);
        var estate = (Estate)Map.EntitiesDict[message.EstateUuid];
        bool isConfirm = await _dialogService.ShowDialogAsync(new MessageDialogViewModel
        {
            Title = "是否购买该资产", Message = $"购买{estate.Title}需要{estate.Value}", IsShowCancelButton = true,
            IsLightDismissEnabled = false
        });
        if (isConfirm)
            await _gameManager.NetClientInstance.SendRequestAsync(new BuyEstateRequest(message.Id,_gameManager.LocalPlayer.Uuid, estate.Uuid));
        else
            await _gameManager.NetClientInstance.SendRequestAsync(new BuyEstateRequest());// 和服务器表示不买
    }


    private void ReceiveTurnStartMessage(object sender, TurnStartResponse message)
    {
        IsShowReminderText = false;
        if (message.PlayerUuid == _gameManager.LocalPlayer.Uuid)
            IsShowReminderText = true;
    }

    private void ReceiveInitGameMessage(object sender, InitGameResponse message)
    {
        foreach (var localPlayerData in _gameManager.Players)
        {
            foreach (var remotePlayerData in message.Players)
            {
                if (localPlayerData.Uuid != remotePlayerData.Uuid) continue;
                localPlayerData.CurrentNodeUuId = remotePlayerData.CurrentNodeUuId;
                localPlayerData.Money = remotePlayerData.Money;
            }
        }
        
        if (message.Players.ToArray()[message.CurrentTurnPlayerIndex] != _gameManager.LocalPlayer) return;
        IsShowReminderText = false;
        IsShowReminderText = true;
    }

    private async void ReceiveRollDiceMessage(object sender, RollDiceResponse message)
    {
        if (message.ResponseStatus!= RequestResult.Success)
        {
            await _dialogService.ShowDialogAsync(new MessageDialogViewModel
            {
                Title = "错误",
                Message = "投骰子失败，可能现在还没轮到你"
            });
        }
        else
            RollDiceValue = message.DiceValue;
    }

    private void ReceiveUpdateEstateInfoMessage(object sender, UpdateEstateInfoResponse message)
    {
        var estate = (Estate)Map.EntitiesDict[message.EstateUuid];
        estate.Owner = _gameManager.GetPlayerByUuid(message.OwnerUuid);
        estate.Level = message.Level;
    }
}