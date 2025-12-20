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
public partial class GameRuleService : ObservableObject
{
  private readonly GameManager _gameManager;
  private readonly DialogService _dialogService;
  
  // 应该在回合结束时播放音效

  public GameRuleService(GameManager gameManager, DialogService dialogService, LibVLC libVlc,
    RoleStrategyFactory strategyFactory)
  {
    _gameManager = gameManager;
    _dialogService = dialogService;
    WeakReferenceMessenger.Default.Register<PlayerMoveMessage>(this,ReceivePlayerMove);
  }

  private async void ReceivePlayerMove(object recipient, PlayerMoveMessage message)
  {
    if (_gameManager.LocalPlayer.IsRoomOwner)
      await HandleStepOnNodeAsync(_gameManager.LocalPlayer, (NodeModel)_gameManager.SelectedMap.EntitiesDict[message.DestinationNodeUuid]);
  }


  public async Task HandleStepOnNodeAsync(Player player,NodeModel node)// 应该是只给服务器调用，客户端接收回应后在显示可用操作
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
  }

  /// <summary>
  /// 处理踩到地产格子的方法
  /// </summary>
  /// <param name="estate">地产</param>
  /// <param name="player">玩家</param>
  private async Task HandleEstateAsync( Player player,Estate estate)
  {
    if (estate.Owner == null && player.Money >= estate.Value)
    {
      if (player.IsRoomOwner)
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
        WeakReferenceMessenger.Default.Send(new UpdateEstateInfoMessage(player.Uuid,
          estate.Uuid, 1));
      }
      else
      {
        var buyEstateActionMessage = new BuyEstateAction(estate.Uuid);
        var client = _gameManager.GetClientByPlayerUuid(player.Uuid);
        var buyEstateRequest = await _gameManager.NetServerInstance.SendRequestAsync<BuyEstateAction,BuyEstateRequest>(buyEstateActionMessage,client);
        if (buyEstateRequest.IsConfirm)
        {
          player.Money -= estate.Value;
          estate.Owner = player;
          var message = new UpdateEstateInfoResponse(player.Uuid, estate.Uuid);
          await _gameManager.NetServerInstance.Broadcast(message);
          WeakReferenceMessenger.Default.Send(new UpdateEstateInfoMessage(player.Uuid,
            estate.Uuid, 1));
        }
      }
    }
  }
  
  private Task HandleSpawnPointAsync(Player player)
  {
    player.Money += _gameManager.SelectedMap.SpawnPointCashReward;
    return Task.CompletedTask;
  }
}