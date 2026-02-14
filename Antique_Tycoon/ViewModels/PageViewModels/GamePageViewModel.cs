using System.Linq;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Net.Tcp.Response.GameAction;
using Antique_Tycoon.Models.Nodes;
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

  private readonly ActionQueueService _actionQueue = App.Current.Services.GetRequiredService<ActionQueueService>();

  // private readonly GameRuleService _gameRuleService = App.Current.Services.GetRequiredService<GameRuleService>();
  private readonly DialogService _dialogService = App.Current.Services.GetRequiredService<DialogService>();
  [ObservableProperty] private int _rollDiceValue;

  public GamePageViewModel(Map map)
  {
    Map = map;
    foreach (var player in _gameManager.Players)
      Map.SpawnNode.PlayersHere.Add(player);

    // WeakReferenceMessenger.Default.Register<NodeClickedMessage>(this, ReceiveNodeClicked);
    WeakReferenceMessenger.Default.Register<InitGameResponse>(this, ReceiveInitGameMessage);
    WeakReferenceMessenger.Default.Register<RollDiceResponse>(this, ReceiveRollDiceMessage);
    WeakReferenceMessenger.Default.Register<UpdateEstateInfoResponse>(this, ReceiveUpdateEstateInfoMessage);
    WeakReferenceMessenger.Default.Register<SelectDestinationAction>(this, ReceiveSelectDestinationAction);
  }

  private void ReceiveHireStaffResponse(object recipient, HireStaffResponse message)
  {
    
  }


  private void ReceiveSelectDestinationAction(object recipient, SelectDestinationAction message)
  {
    WeakReferenceMessenger.Default.Send(new GameMaskShowMessage(message.Destinations,
      Map)); //转发一下消息，因为GameMaskShowMessage是可等待的消息，SelectDestinationAction已经继承了其他类型
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
  }

  private async void ReceiveRollDiceMessage(object sender, RollDiceResponse message)
  {
    if (message.ResponseStatus != RequestResult.Success)
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