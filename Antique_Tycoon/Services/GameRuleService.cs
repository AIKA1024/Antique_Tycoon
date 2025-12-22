using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Net.Tcp.Response.GameAction;
using Antique_Tycoon.Models.Node;
using Antique_Tycoon.ViewModels.DialogViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LibVLCSharp.Shared;
using Player = Antique_Tycoon.Models.Player;

namespace Antique_Tycoon.Services;

/// <summary>
/// 游戏规则核心服务，封装所有玩法逻辑
/// </summary>
public class GameRuleService : ObservableObject
{
  private readonly GameManager _gameManager;
  private readonly DialogService _dialogService;
  
  // 应该在回合结束时播放音效

  public GameRuleService(GameManager gameManager, DialogService dialogService, LibVLC libVlc,
    RoleStrategyFactory strategyFactory)
  {
    _gameManager = gameManager;
    _dialogService = dialogService;
    WeakReferenceMessenger.Default.Register<PlayerMoveResponse>(this,ReceivePlayerMove);
  }

  private async void ReceivePlayerMove(object recipient, PlayerMoveResponse message)
  {
    if (_gameManager.IsRoomOwner)
      await HandleStepOnNodeAsync(_gameManager.GetPlayerByUuid(message.PlayerUuid), (NodeModel)_gameManager.SelectedMap.EntitiesDict[message.DestinationNodeUuid]);
  }


  private async Task HandleStepOnNodeAsync(Player player,NodeModel node)// 应该是只给服务器调用，客户端接收回应后在显示可用操作
  {
    switch (node)
    {
      case Estate estate:
        await HandleEstateAsync(player,estate);
        break;
      case SpawnPoint:
        await HandleSpawnPointAsync(player);
        break;
    }
    await _gameManager.AdvanceToNextPlayerTurnAsync();
  }

  /// <summary>
  /// 处理踩到地产格子的方法
  /// </summary>
  /// <param name="estate">地产</param>
  /// <param name="player">玩家</param>
  private async Task HandleEstateAsync(Player player,Estate estate)
  {
    if (estate.Owner == null && player.Money >= estate.Value)
    {
      if (_gameManager.RoomOwnerUuid == player.Uuid)
      {
        bool isConfirm = await _dialogService.ShowDialogAsync(new MessageDialogViewModel
        {
          Title = "是否购买该资产", Message = $"购买{estate.Title}需要{estate.Value}", IsShowCancelButton = true,
          IsLightDismissEnabled = false
        });
        if (!isConfirm) return;
        player.Money -= estate.Value;
        var message = new UpdateEstateInfoResponse(player.Uuid, estate.Uuid);
        await _gameManager.NetServerInstance.Broadcast(message);
        WeakReferenceMessenger.Default.Send(message);
      }
      else
      {
        var buyEstateActionMessage = new BuyEstateAction(estate.Uuid);
        var client = _gameManager.GetClientByPlayerUuid(player.Uuid);
        try
        {
          var buyEstateRequest =
            await _gameManager.NetServerInstance.SendRequestAsync<BuyEstateAction, BuyEstateRequest>(
              buyEstateActionMessage, client,TimeSpan.FromSeconds(10));
          if (buyEstateRequest.IsConfirm)
          {
            player.Money -= estate.Value;
            estate.Owner = player;
            var message = new UpdateEstateInfoResponse(player.Uuid, estate.Uuid);
            await _gameManager.NetServerInstance.Broadcast(message);
            WeakReferenceMessenger.Default.Send(message);
          }
        }
        catch (OperationCanceledException e)
        {
          
        }
      }
    }
  }
  
  private async Task HandleSpawnPointAsync(Player player)//todo 不正确，应该路过就给钱，而不是踩到，并且踩到就炸了，不知道为什么
  {
    var bonus= _gameManager.SelectedMap.SpawnPointCashReward;
    player.Money += bonus;
      var message =
        new UpdatePlayerInfoResponse(player, $"{player.Name}路过了出生点，获得{bonus} {player.Money - bonus}->{player.Money}");
      await _gameManager.NetServerInstance.Broadcast(message);
      WeakReferenceMessenger.Default.Send(message);
  }
}