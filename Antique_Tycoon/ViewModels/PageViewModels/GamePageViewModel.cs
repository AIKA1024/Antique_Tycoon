using System.Linq;
using System.Threading.Tasks;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
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
    private readonly GameRuleService _gameRuleService = App.Current.Services.GetRequiredService<GameRuleService>();
    private readonly DialogService _dialogService = App.Current.Services.GetRequiredService<DialogService>();

    [ObservableProperty] private string _reminderText = "轮到你啦";
    [ObservableProperty] private int _rollDiceValue;
    [ObservableProperty] private bool _isShowReminderText;

    private TaskCompletionSource<string>? _nodeClickTcs;

    /// <summary>
    /// 是否在高亮给玩家选择卡片阶段
    /// </summary>
    private bool _isHighlightMode;

    public GamePageViewModel(Map map)
    {
        Map = map;
        foreach (var player in _gameManager.Players)
            Map.SpawnNode.PlayersHere.Add(player);

        WeakReferenceMessenger.Default.Register<NodeClickedMessage>(this, ReceiveNodeClicked);
        WeakReferenceMessenger.Default.Register<TurnStartMessage>(this, ReceiveTurnStartMessage);
        WeakReferenceMessenger.Default.Register<InitGameMessage>(this, ReceiveInitGameMessage);
        WeakReferenceMessenger.Default.Register<RollDiceMessage>(this, ReceiveRollDiceMessage);
        WeakReferenceMessenger.Default.Register<PlayerMoveMessage>(this, ReceivePlayerMoveMessage);
        WeakReferenceMessenger.Default.Register<UpdateEstateInfoMessage>(this, ReceiveUpdateEstateInfoMessage);
    }

    private async Task<string> AwaitNodeClickAsync()
    {
        // 2. 初始化 TaskCompletionSource，用于阻塞等待
        _nodeClickTcs = new TaskCompletionSource<string>();

        // 3. 等待点击结果（非 UI 阻塞，可 await）
        var selectedCardUuid = await _nodeClickTcs.Task;

        // 5. 返回选中的卡片 UUID
        return selectedCardUuid;
    }


    private void ReceiveNodeClicked(object sender, NodeClickedMessage message)
    {
        // 过滤无效点击：仅处理「高亮模式下」的点击
        if (!_isHighlightMode) return;

        // 触发 TaskCompletionSource 完成，返回结果
        _nodeClickTcs?.SetResult(message.NodeUuid);
        _nodeClickTcs = null; // 重置，避免重复触发
    }

    private void ReceiveTurnStartMessage(object sender, TurnStartMessage message)
    {
        IsShowReminderText = false;
        if (message.Value == _gameManager.LocalPlayer)
            IsShowReminderText = true;
    }

    private void ReceiveInitGameMessage(object sender, InitGameMessage message)
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

    private async void ReceiveRollDiceMessage(object sender, RollDiceMessage message)
    {
        RollDiceValue = message.DiceValue;
        if (message.PlayerUuid!= _gameManager.LocalPlayer.Uuid)
            return;
        
        if (message.Success)
        {
            Player currentPlayer = _gameManager.GetPlayerByUuid(message.PlayerUuid);
            var selectableNodes =
                Map.GetNodesAtExactStepViaActiveConnections(currentPlayer.CurrentNodeUuId, message.DiceValue).ToArray();
            WeakReferenceMessenger.Default.Send(new GameMaskShowMessage(true));
            // await _dialogService.ShowDialogAsync(new MessageDialogViewModel
            // {
            //     Title = "可以选择的格子",
            //     Message = string.Join(",", selectableNodes.Select(n => n.Title))
            // });
            _isHighlightMode = true;
            foreach (var node in selectableNodes)
                node.ZIndex = 4;

            var selectedNodeUuid = await AwaitNodeClickAsync();
            await _gameRuleService.PlayerMove(selectedNodeUuid);
            foreach (var node in selectableNodes)
                node.ZIndex = 1;
            _isHighlightMode = false;
            WeakReferenceMessenger.Default.Send(new GameMaskShowMessage(false));
        }
        else
        {
            await _dialogService.ShowDialogAsync(new MessageDialogViewModel
            {
                Title = "错误",
                Message = "投骰子失败，可能现在还没轮到你"
            });
        }
    }

    private void ReceivePlayerMoveMessage(object sender, PlayerMoveMessage message)
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