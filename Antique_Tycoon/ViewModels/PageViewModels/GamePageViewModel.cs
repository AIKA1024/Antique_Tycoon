using System.Linq;
using System.Threading.Tasks;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
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
        WeakReferenceMessenger.Default.Register<TurnStartMessage>(this, ReceiveTurnStartMessage);
        WeakReferenceMessenger.Default.Register<InitGameMessageResponse>(this, ReceiveInitGameMessage);
        WeakReferenceMessenger.Default.Register<RollDiceMessage>(this, ReceiveRollDiceMessage);
        WeakReferenceMessenger.Default.Register<PlayerMoveMessage>(this, ReceivePlayerMoveMessage);
        WeakReferenceMessenger.Default.Register<UpdateEstateInfoMessage>(this, ReceiveUpdateEstateInfoMessage);
        WeakReferenceMessenger.Default.Register<BuyEstateAction>(this ,ReceiveBuyEstateAction);
    }

    private async void ReceiveBuyEstateAction(object recipient, BuyEstateAction message)
    {
        var estate = (Estate)Map.EntitiesDict[message.EstateUuid];
        bool isConfirm = await _dialogService.ShowDialogAsync(new MessageDialogViewModel
        {
            Title = "是否购买该资产", Message = $"购买{estate.Title}需要{estate.Value}", IsShowCancelButton = true,
            IsLightDismissEnabled = false
        });
        if (isConfirm)
            await _gameManager.NetClientInstance.SendRequestAsync(new BuyEstateRequest(message.Id,_gameManager.LocalPlayer.Uuid, estate.Uuid));//扣钱逻辑让服务器发送更新金额要求
        else
            await _gameManager.NetClientInstance.SendRequestAsync(new BuyEstateRequest());// 和服务器表示不买
    }


    private void ReceiveTurnStartMessage(object sender, TurnStartMessage message)
    {
        IsShowReminderText = false;
        if (message.Value == _gameManager.LocalPlayer.Uuid)
            IsShowReminderText = true;
    }

    private void ReceiveInitGameMessage(object sender, InitGameMessageResponse message)
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

    private void ReceiveRollDiceMessage(object sender, RollDiceMessage message)
    {
        RollDiceValue = message.DiceValue;
    }

    private async void ReceivePlayerMoveMessage(object sender, PlayerMoveMessage message)//todo 踩到格子后有什么操作应该是服务器发送后再执行，而不是自己本地判断
    {
        Player player = _gameManager.GetPlayerByUuid(message.PlayerUuid);
        string playerCurrentNodeUuid = player.CurrentNodeUuId;
        NodeModel currentModelmodel = (NodeModel)Map.EntitiesDict[playerCurrentNodeUuid];
        NodeModel destinationModelmodel = (NodeModel)Map.EntitiesDict[message.DestinationNodeUuid];
        currentModelmodel.PlayersHere.Remove(player);
        destinationModelmodel.PlayersHere.Add(player);
        player.CurrentNodeUuId = destinationModelmodel.Uuid;
    }

    private void ReceiveUpdateEstateInfoMessage(object sender, UpdateEstateInfoMessage message)
    {
        var estate = (Estate)Map.EntitiesDict[message.EstateUuid];
        estate.Owner = _gameManager.GetPlayerByUuid(message.PlayerUuid);
        estate.Level = message.Level;
    }
}