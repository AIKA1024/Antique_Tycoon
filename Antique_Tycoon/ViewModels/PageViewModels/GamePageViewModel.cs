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
    
    WeakReferenceMessenger.Default.Register<NodeClickedMessage>(this,ReceiveNodeClicked);
    
    WeakReferenceMessenger.Default.Register<TurnStartMessage>(this, (_, message) =>
    {
      IsShowReminderText = false;
      if (message.Value == _gameManager.LocalPlayer)
        IsShowReminderText = true;
    });
    WeakReferenceMessenger.Default.Register<InitGameMessage>(this, (_, message) =>
    {
      if (message.Players.ToArray()[message.CurrentTurnPlayerIndex] != _gameManager.LocalPlayer) return;
      IsShowReminderText = false;
      IsShowReminderText = true;
    });
    WeakReferenceMessenger.Default.Register<RollDiceMessage>(this,async (_, message) =>
    {
      if (message.Success)
      {
        RollDiceValue = message.DiceValue;
        Player currentPlayer = _gameManager.GetPlayerByUuid(message.PlayerUuid);
        var selectableNodes =
          Map.GetNodesAtExactStepViaActiveConnections(currentPlayer.CurrentNodeUuId, message.DiceValue).ToArray();

        await _dialogService.ShowDialogAsync(new MessageDialogViewModel()
        {
            Title = "可以选择的格子",
            Message = string.Join(",", selectableNodes.Select(n=>n.Title))
        });
        _isHighlightMode = true;
        foreach (var node in selectableNodes)
        {
          node.ZIndex = 2;
        }
        
        //todo 这里可以有多个路线可以走，应该显示ui让玩家选择一下，才知道目的地的uuid是多少,还要和服务器再通信一次
        var selectedNodeUuid = await AwaitNodeClickAsync();
        await _dialogService.ShowDialogAsync(new MessageDialogViewModel()
        {
          Title = "选择结果",
          Message = $"你选择了{((NodeModel)map.EntitiesDict[selectedNodeUuid]).Title}"
        });
        // WeakReferenceMessenger.Default.Send(
        //   new PlayerMoveMessage(currentPlayer,currentPlayer.CurrentNodeUuId)
        // );
        _isHighlightMode = false;
      }
      else
      {
        await _dialogService.ShowDialogAsync(new MessageDialogViewModel
        {
          Title = "错误",
          Message = "投骰子失败，可能现在还没轮到你"
        });
      }
    });
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

  
  private void ReceiveNodeClicked(object sender,NodeClickedMessage message)
  {
    // 过滤无效点击：仅处理「高亮模式下」的点击
    if (!_isHighlightMode) return;

    // 触发 TaskCompletionSource 完成，返回结果
    _nodeClickTcs?.SetResult(message.NodeUuid);
    _nodeClickTcs = null; // 重置，避免重复触发
  }
}