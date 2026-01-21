using System;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Net.Tcp.Response.GameAction;
using Antique_Tycoon.Models.Nodes;
using Antique_Tycoon.ViewModels.DialogViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LibVLCSharp.Shared;
using Mine = Antique_Tycoon.Models.Nodes.Mine;
using NodeModel = Antique_Tycoon.Models.Nodes.NodeModel;
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

  private async Task<RollDiceResponse> GetRollDiceAsync(TcpClient? client)
  {
    int rollDiceValue = Random.Shared.Next(1, 7);
    RollDiceResponse rollDiceResponse = new RollDiceResponse("", _gameManager.CurrentTurnPlayer.Uuid, rollDiceValue);
    try
    {
      var rollDiceRequest =
        await _gameManager.NetServerInstance
          .SendRequestAsync<RollDiceAction, RollDiceRequest>(new RollDiceAction(), client);
      rollDiceValue = Random.Shared.Next(1, 7); //为了让点数真正是玩家请求后随机
      rollDiceResponse = new RollDiceResponse(rollDiceRequest.Id, _gameManager.CurrentTurnPlayer.Uuid, rollDiceValue);
      await _gameManager.NetServerInstance.Broadcast(rollDiceResponse);
      WeakReferenceMessenger.Default.Send(rollDiceResponse);
    }
    catch (TimeoutException e)
    {
      Console.WriteLine("玩家回合超时");
    }

    return rollDiceResponse;
  }

  private async Task StartGameRule()
  {
    if (!_gameManager.IsRoomOwner)
      return;

    await Task.Yield(); //todo 先这样等待一下ui注册事件了先

    while (!_gameManager.IsGameOver)
    {
      string currentTurnPlayerUuid = _gameManager.CurrentTurnPlayer.Uuid;
      var client = _gameManager.GetClientByPlayerUuid(_gameManager.CurrentTurnPlayer.Uuid);
      var rollDiceResponse = await GetRollDiceAsync(client);
      var nodeDic = _gameManager.SelectedMap.GetPathsAtExactStep(_gameManager.CurrentTurnPlayer.CurrentNodeUuId,
        rollDiceResponse.DiceValue);
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
        catch (TimeoutException e)
        {
          Console.WriteLine("玩家选择目的地超时，默认第一个");
        }
      }

      var playerMoveResponse = new PlayerMoveResponse(currentTurnPlayerUuid, selectPath);
      WeakReferenceMessenger.Default.Send(playerMoveResponse);
      await _gameManager.NetServerInstance.Broadcast(playerMoveResponse);

      await HandleStepOnNodeAsync(
        _gameManager.GetPlayerByUuid(currentTurnPlayerUuid),
        (NodeModel)_gameManager.SelectedMap.EntitiesDict[selectPath[^1]]);

      await AdvanceToNextPlayerTurnAsync();
    }
  }


  /// <summary>
  /// 处理踩到格子的逻辑（修改返回值：是否结束当前玩家回合）
  /// </summary>
  /// <param name="player">玩家</param>
  /// <param name="node">踩到的格子</param>
  /// <returns>是否结束当前回合</returns>
  private async Task<bool> HandleStepOnNodeAsync(Player player, NodeModel node)
  {
    switch (node)
    {
      case Estate estate:
        await HandleEstateAsync(player, estate);
        // 普通地产格子：处理完后回合结束
        return true;

      case SpawnPoint:
        await HandleSpawnPointAsync(player);
        // 出生点：处理完后回合结束（你可根据需求修改为 false，比如路过出生点可再投骰子）
        return true;

      case Mine:
        await HandleMineAsync(player, node);
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
  private async Task HandleEstateAsync(Player player, Estate estate)
  {
    var client = _gameManager.GetClientByPlayerUuid(player.Uuid);
    if (estate.Owner == null) //踩到还没人买的地
    {
      if (player.Money >= estate.Value)
      {
        var buyEstateActionMessage = new BuyEstateAction(estate.Uuid);
        try
        {
          var buyEstateRequest =
            await _gameManager.NetServerInstance.SendRequestAsync<BuyEstateAction, BuyEstateRequest>(
              buyEstateActionMessage, client);
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
    else if (estate.Owner == player) //踩到自己的地
      await SaleAntique(player, "");
    else //踩到别人的地
      await SaleAntique(estate.Owner, player.Uuid);

    return;

    async Task SaleAntique(Player seller, string buyerUuid)
    {
      if (seller.Antiques.Count > 0)
      {
        var saleAntiqueAction = new SaleAntiqueAction(seller.Uuid, buyerUuid, estate.Uuid); //购买者空字符串代表银行
        var saleAntiqueRequest =
          await _gameManager.NetServerInstance.SendRequestAsync<SaleAntiqueAction, SaleAntiqueRequest>(
            saleAntiqueAction, client);

        if (string.IsNullOrEmpty(saleAntiqueRequest.AntiqueUuid))
          return;

        var antique = seller.Antiques.First(a => a.Uuid == saleAntiqueRequest.AntiqueUuid);
        if (saleAntiqueRequest.IsUpgradeEstate)
        {
          estate.Level += 1;
          var updateEstateInfoResponse = new UpdateEstateInfoResponse(seller.Uuid, estate.Uuid, estate.Level);
          await Broadcast(updateEstateInfoResponse);
          var updatePlayerInfoResponse = new UpdatePlayerInfoResponse(seller,
            $"{seller.Name}原价出售古董，获得${seller.Money += antique.Value},{estate.Title}等级提升为{estate.Level}");
          await Broadcast(updatePlayerInfoResponse);
        }
        else
        {
          seller.Money += estate.CalculateCurrentRevenue(antique.Value);
          var updatePlayerInfoResponse = new UpdatePlayerInfoResponse(seller,
            $"{seller.Name}加价出售古董，获得${seller.Money += antique.Value}");
          await Broadcast(updatePlayerInfoResponse);
        }

        seller.Antiques.Remove(antique);
      }
    }
  }

  private async Task HandleSpawnPointAsync(Player player)
  {
    var bonus = _gameManager.SelectedMap.SpawnPointCashReward;
    var message =
      new UpdatePlayerInfoResponse(player, $"{player.Name}路过了出生点，获得{bonus} {player.Money}->{player.Money += bonus}");
    await Broadcast(message);
  }

  private async Task HandleMineAsync(Player player, NodeModel node)
  {
    if (_gameManager.SelectedMap.Antiques.Count > 0)
    {
      var randomIndex = Random.Shared.Next(0, _gameManager.SelectedMap.Antiques.Count);
      var antique = _gameManager.SelectedMap.Antiques[randomIndex];
      var antiqueChangeResponse = new AntiqueChanceResponse(antique.Uuid, player.Uuid, node.Uuid);
      var client = _gameManager.GetClientByPlayerUuid(player.Uuid);
      WeakReferenceMessenger.Default.Send(antiqueChangeResponse, node.Uuid);
      await Broadcast(antiqueChangeResponse);
      var rollDiceResponse = await GetRollDiceAsync(client);
      var isSucceed = rollDiceResponse.DiceValue >= antique.Dice;
      var getAntiqueResultResponse =
        new GetAntiqueResultResponse(antique.Uuid, player.Uuid, node.Uuid, isSucceed);
      WeakReferenceMessenger.Default.Send(getAntiqueResultResponse, node.Uuid);
      await Broadcast(getAntiqueResultResponse); //todo 客户端好像还在等待
      if (isSucceed)
      {
        player.Antiques.Add(antique);
        await Broadcast(new UpdatePlayerInfoResponse(player, $"{player.Name}成功获得{antique.Name}"));
      }

      _gameManager.SelectedMap.Antiques.RemoveAt(randomIndex); //todo 直接修改地图的古玩数组可能不太好
    }
    else
    {
      var message =
        new UpdatePlayerInfoResponse(player, $"已经没有古玩流通，因此{player.Name}获得200 {player.Money}->{player.Money += 200}");
      await Broadcast(message);
    }
  }

  private async Task Broadcast(ResponseBase response)
  {
    await _gameManager.NetServerInstance.Broadcast(response);
    WeakReferenceMessenger.Default.Send(response);
  }

  /// <summary>
  /// 玩家请求投骰子
  /// </summary>
  /// <param name="actionMessageId">服务器的行动id</param>
  public async Task RollDiceAsync(string actionMessageId)
  {
    await _gameManager.SendToGameServerAsync(new RollDiceRequest(actionMessageId));
  }
}