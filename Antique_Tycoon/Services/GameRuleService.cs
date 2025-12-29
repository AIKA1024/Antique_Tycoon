using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Extensions;
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
  private readonly AnimationManager _animationManager;
  private readonly TaskCompletionSource<int> GetRollDiceValueTCS = new();

  // 应该在回合结束时播放音效


  public GameRuleService(GameManager gameManager, DialogService dialogService, LibVLC libVlc,
    RoleStrategyFactory strategyFactory, AnimationManager animationManager)
  {
    _gameManager = gameManager;
    _dialogService = dialogService;
    _animationManager = animationManager;
    // WeakReferenceMessenger.Default.Register<PlayerMoveResponse>(this, ReceivePlayerMove); //不是viewmodel就别再用信使通信了
    WeakReferenceMessenger.Default.Register<StartGameResponse>(this, async (_, _) => await StartGameRule());
  }

  public async Task AdvanceToNextPlayerTurnAsync()
  {
    _gameManager.CurrentTurnPlayerIndex = (_gameManager.CurrentTurnPlayerIndex + 1) % _gameManager.Players.Count;
    var turnStartResponse = new TurnStartResponse { PlayerUuid = _gameManager.CurrentTurnPlayer.Uuid };
    await _gameManager.NetServerInstance.Broadcast(turnStartResponse);
    WeakReferenceMessenger.Default.Send(turnStartResponse);
  }

  private async Task StartGameRule()
  {
    if (!_gameManager.IsRoomOwner)
      return;
    
    string currentTurnPlayerUuid = _gameManager.CurrentTurnPlayer.Uuid;
    while (!_gameManager.IsGameOver)
    {
      if (currentTurnPlayerUuid == _gameManager.LocalPlayer.Uuid)
      {
        WeakReferenceMessenger.Default.Send(new RollDiceResponse("",currentTurnPlayerUuid,Random.Shared.Next(1, 7)));
      }
      
      var client = _gameManager.GetClientByPlayerUuid(currentTurnPlayerUuid);
      //默认都是玩家想要移动，开始投骰子
      int rollDiceValue = Random.Shared.Next(1, 7);
      RollDiceResponse rollDiceResponse = new RollDiceResponse("", currentTurnPlayerUuid, rollDiceValue);
      try
      {
        var rollDiceRequest =
          await _gameManager.NetServerInstance
            .SendRequestAsync<RollDiceAction, RollDiceRequest>(new RollDiceAction(), client);
        rollDiceResponse = new RollDiceResponse(rollDiceRequest.Id, currentTurnPlayerUuid, rollDiceValue);
      }
      catch (OperationCanceledException e)
      {
        Console.WriteLine("玩家回合超时");
      }

      await _gameManager.NetServerInstance.Broadcast(rollDiceResponse);
      var nodeDic = _gameManager.SelectedMap.GetPathsAtExactStep(_gameManager.CurrentTurnPlayer.CurrentNodeUuId,
        rollDiceValue);
      var selectPath = nodeDic.First().Value;

      if (nodeDic.Count > 1)
      {
        var selectDestinationAction = new SelectDestinationAction(nodeDic.Keys.ToList());
        try
        {
          var selectDestinationRequest = await
            _gameManager.NetServerInstance.SendRequestAsync<SelectDestinationAction, SelectDestinationRequest>(
              selectDestinationAction, client);
          selectPath = nodeDic[selectDestinationRequest.DestinationUuid];
        }
        catch (OperationCanceledException e)
        {
          Console.WriteLine("玩家选择目的地超时，默认第一个");
        }
      }
      var playerMoveResponse = new PlayerMoveResponse(currentTurnPlayerUuid, selectPath);
      await _gameManager.NetServerInstance.Broadcast(playerMoveResponse);

      await HandleStepOnNodeAsync(
        _gameManager.GetPlayerByUuid(currentTurnPlayerUuid),
        (NodeModel)_gameManager.SelectedMap.EntitiesDict[selectPath[^1]],
        playerMoveResponse.Id);

      await AdvanceToNextPlayerTurnAsync();
    }
  }

  /// <summary>
  /// 接收玩家移动响应（核心入口，添加锁保护）
  /// </summary>
  // private async void ReceivePlayerMove(object recipient, PlayerMoveResponse message)
  // {
  //   // 1. 快速校验：非当前玩家直接拒绝
  //   var currentPlayer = _gameManager.CurrentTurnPlayer;
  //   if (currentPlayer == null || currentPlayer.Uuid != message.PlayerUuid)
  //   {
  //     Console.WriteLine($"玩家{message.PlayerUuid}非当前回合玩家，拒绝移动请求");
  //     return;
  //   }
  //
  //   // 2. 异步锁：防止同一玩家重复请求（3秒超时，避免死等）
  //   if (!await _gameManager.GameActionLock.WaitAsync(TimeSpan.FromSeconds(3)))
  //   {
  //     Console.WriteLine($"玩家{message.PlayerUuid}请求繁忙，拒绝重复移动请求");
  //     return;
  //   }
  //
  //   try
  //   {
  //     // 3. 锁内二次校验：防止等待期间玩家已切换
  //     if (_gameManager.CurrentTurnPlayer.Uuid != message.PlayerUuid)
  //     {
  //       Console.WriteLine($"玩家{message.PlayerUuid}回合已切换，拒绝移动请求");
  //       return;
  //     }
  //
  //     // 4. 执行核心逻辑，并获取回合是否结束的结果
  //     bool isTurnFinished = await HandleStepOnNodeAsync(
  //       _gameManager.GetPlayerByUuid(message.PlayerUuid),
  //       (NodeModel)_gameManager.SelectedMap.EntitiesDict[message.Path[^1]],
  //       message.Id);
  //
  //     // 5. 仅当回合结束时，才切换到下一个玩家（核心调整）
  //     if (isTurnFinished)
  //     {
  //       await AdvanceToNextPlayerTurnAsync();
  //       Console.WriteLine($"玩家{message.PlayerUuid}回合结束，切换到下一个玩家");
  //     }
  //     else
  //     {
  //       Console.WriteLine($"玩家{message.PlayerUuid}回合未结束，可继续操作");
  //     }
  //   }
  //   catch (Exception e)
  //   {
  //     Console.WriteLine($"处理玩家移动失败：{e.Message}");
  //   }
  //   finally
  //   {
  //     // 6. 释放锁：无论成功/失败，必须释放
  //     _gameManager.GameActionLock.Release();
  //   }
  // }

  /// <summary>
  /// 处理踩到格子的逻辑（修改返回值：是否结束当前玩家回合）
  /// </summary>
  /// <param name="player">玩家</param>
  /// <param name="node">踩到的格子</param>
  /// <param name="animationToken">动画标识</param>
  /// <returns>是否结束当前回合</returns>
  private async Task<bool> HandleStepOnNodeAsync(Player player, NodeModel node, string animationToken)
  {
    switch (node)
    {
      case Estate estate:
        await HandleEstateAsync(player, estate, animationToken);
        // 普通地产格子：处理完后回合结束
        return true;

      case SpawnPoint:
        await HandleSpawnPointAsync(player);
        // 出生点：处理完后回合结束（你可根据需求修改为 false，比如路过出生点可再投骰子）
        return true;

      // 扩展：可添加其他格子类型（比如抽奖/双倍骰子格子），返回 false 表示回合不结束
      // case LotteryNode lotteryNode:
      //     await HandleLotteryAsync(player, lotteryNode);
      //     return false; // 抽奖后可继续投骰子

      default:
        // 未知格子：默认结束回合
        return true;
    }
  }

  /// <summary>
  /// 处理踩到地产格子的方法
  /// </summary>
  /// <param name="player">玩家</param>
  /// <param name="estate">地产</param>
  /// <param name="animationToken">要等待的动画token</param>
  private async Task HandleEstateAsync(Player player, Estate estate, string animationToken)
  {
    if (estate.Owner == null && player.Money >= estate.Value)
    {
      if (_gameManager.RoomOwnerUuid == player.Uuid)
      {
        await _animationManager.WaitAnimation(animationToken);
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
        var buyEstateActionMessage = new BuyEstateAction(animationToken, estate.Uuid);
        var client = _gameManager.GetClientByPlayerUuid(player.Uuid);
        try
        {
          var buyEstateRequest =
            await _gameManager.NetServerInstance.SendRequestAsync<BuyEstateAction, BuyEstateRequest>(
              buyEstateActionMessage, client, TimeSpan.FromSeconds(30));
          if (buyEstateRequest.IsConfirm)
          {
            player.Money -= estate.Value;
            estate.Owner = player;
            var message = new UpdateEstateInfoResponse(player.Uuid, estate.Uuid);
            await _gameManager.NetServerInstance.Broadcast(message);
            WeakReferenceMessenger.Default.Send(message);
          }
        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message);
        }
      }
    }
  }

  private async Task HandleSpawnPointAsync(Player player)
  {
    var bonus = _gameManager.SelectedMap.SpawnPointCashReward;
    player.Money += bonus;
    var message =
      new UpdatePlayerInfoResponse(player, $"{player.Name}路过了出生点，获得{bonus} {player.Money - bonus}->{player.Money}");
    await _gameManager.NetServerInstance.Broadcast(message);
    WeakReferenceMessenger.Default.Send(message);
  }
}