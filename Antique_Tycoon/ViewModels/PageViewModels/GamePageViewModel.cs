using System.Linq;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
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
  public GamePageViewModel(Map map)
  {
    Map = map;
    foreach (var player in _gameManager.Players)
      Map.SpawnNode.PlayersHere.Add(player);
    
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
          Map.GetNodesAtExactStepViaActiveConnections(currentPlayer.CurrentNodeUuId, message.DiceValue);

        await _dialogService.ShowDialogAsync(new MessageDialogViewModel()
        {
            Title = "可以选择的格子",
            Message = string.Join(",", selectableNodes.Select(n=>n.Title))
        });

        foreach (var node in selectableNodes)
        {
          node.ZIndex = 2;
        }
        
        //todo 这里可以有多个路线可以走，应该显示ui让玩家选择一下，才知道目的地的uuid是多少,还要和服务器再通信一次
        
        // WeakReferenceMessenger.Default.Send(
        //   new PlayerMoveMessage(currentPlayer,currentPlayer.CurrentNodeUuId)
        // );
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
}