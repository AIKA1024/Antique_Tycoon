using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Models.Effects.Contexts;
using Antique_Tycoon.Models.Entities;
using Antique_Tycoon.Models.Enums;
using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Net.Tcp.Response.GameAction;
using Antique_Tycoon.Models.Nodes;
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
  // 应该在回合结束时播放音效


  public GameRuleService(GameManager gameManager, LibVLC libVlc,
    RoleStrategyFactory strategyFactory)
  {
    _gameManager = gameManager;
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
    RollDiceResponse rollDiceResponse =
      new RollDiceResponse("", _gameManager.CurrentTurnPlayer.Uuid, rollDiceValue);
    try
    {
      var rollDiceRequest =
        await _gameManager.NetServerInstance
          .SendRequestAsync<RollDiceAction, RollDiceRequest>(new RollDiceAction(), client);
      rollDiceValue = Random.Shared.Next(1, 7); //为了让点数真正是玩家请求后随机
      rollDiceResponse =
        new RollDiceResponse(rollDiceRequest.Id, _gameManager.CurrentTurnPlayer.Uuid, rollDiceValue);
      await Broadcast(rollDiceResponse);
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
            _gameManager.NetServerInstance
              .SendRequestAsync<SelectDestinationAction, SelectDestinationRequest>(
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
      case TalentMarket:
        await HandleTalentMarketAsync(player, node);
        return true; // 抽奖后可继续投骰子

      default:
        // 未知格子：默认结束回合
        return true;
    }
  }

  private async Task HandleTalentMarketAsync(Player player, NodeModel node)
  {
    var client = _gameManager.GetClientByPlayerUuid(player.Uuid);
    if (_gameManager.Staffs.Count == 0 ||
        player.Money < _gameManager.Staffs.Min(s => s.HiringCost)) //todo 要判断是否至少有一个员工可以被雇佣，而不是直接判断金钱
      return;

    var hireStaffActionMessage = new HireStaffAction(_gameManager.Staffs);
    try
    {
      var hireStaffRequest =
        await _gameManager.NetServerInstance.SendRequestAsync<HireStaffAction, HireStaffRequest>(
          hireStaffActionMessage, client);
      if (string.IsNullOrEmpty(hireStaffRequest.StaffUuid))
        return;

      var staff = _gameManager.Staffs.First(s => s.Uuid == hireStaffRequest.StaffUuid);

      var inventoryMap = player.Antiques.GroupBy(a => a.Index)
        .ToDictionary(g => g.Key, g => g.Count());
      bool canHire = player.Money >= staff.HiringCost &&
                     staff.HiringAntiqueCost.All(cost =>
                       inventoryMap.TryGetValue(cost.Key, out int count) && count >= cost.Value);

      var hireStaffResponse = new HireStaffResponse(hireStaffRequest.Id, player.Uuid, staff.Uuid, canHire);
      await Broadcast(hireStaffResponse);
    }
    catch (Exception e)
    {
      Console.WriteLine(e);
      throw;
    }
  }

  /// <summary>
  /// 处理踩到地产格子的方法
  /// </summary>
  /// <param name="player">玩家</param>
  /// <param name="estate">地产</param>
  private async Task HandleEstateAsync(Player player, Estate estate)//todo 好像踩别人的地，系统误以为是踩地的人卖东西
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
            var message = new UpdateEstateInfoResponse(player.Uuid, estate.Uuid)
              { Id = buyEstateRequest.Id };
            await Broadcast(message);
          }
        }
        catch (Exception e)
        {
          Console.WriteLine(e.Message);
          throw;
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
      if (seller.Antiques.Count == 0) return;

      var saleAntiqueAction = new SaleAntiqueAction(seller.Uuid, buyerUuid, estate.Uuid); //购买者空字符串代表银行
      var saleAntiqueRequest =
        await _gameManager.NetServerInstance.SendRequestAsync<SaleAntiqueAction, SaleAntiqueRequest>(
          saleAntiqueAction, client);

      if (string.IsNullOrEmpty(saleAntiqueRequest.AntiqueUuid))
        return;

      var antique = seller.Antiques.First(a => a.Uuid == saleAntiqueRequest.AntiqueUuid);
      UpdatePlayerMoneyResponse? updatePlayerMoneyRequest = null;
      if (saleAntiqueRequest.IsUpgradeEstate)
      {
        estate.Level += 1;
        var updateEstateInfoResponse =
          new UpdateEstateInfoResponse(seller.Uuid, estate.Uuid, estate.Level)
            { Id = saleAntiqueRequest.Id };
        await Broadcast(updateEstateInfoResponse);
        updatePlayerMoneyRequest =
          new UpdatePlayerMoneyResponse(seller.Uuid, antique.Value, seller.Money + antique.Value)
          {
            LogSegments =
            [
              new LogSegment
              {
                Type = InteractionType.PlayerName,
                Data = seller.Uuid
              },
              new LogSegment
              {
                Text = " 原价出售古玩，获得了"
              },
              new LogSegment
              {
                Text = $" {antique.Value.ToString()}, "
              },
              new LogSegment
              {
                Type = InteractionType.Estate,
                Data = estate.Uuid
              },
              new LogSegment
              {
                Text = $" 等级提升到{estate.Level}"
              }
            ]
          };
      }
      else
      {
        var value = estate.CalculateCurrentRevenue(antique.Value);
        updatePlayerMoneyRequest =
          new UpdatePlayerMoneyResponse(seller.Uuid, value, seller.Money + value)
          {
            Id = saleAntiqueRequest.Id,
            LogSegments =
            [
              new LogSegment
              {
                Type = InteractionType.PlayerName,
                Data = seller.Uuid
              },
              new LogSegment
              {
                Text = " 加价出售古玩，获得了"
              },
              new LogSegment
              {
                Text = $" {estate.CalculateCurrentRevenue(antique.Value)}"
              }
            ]
          };
      }

      seller.Antiques.Remove(antique);
      if (string.IsNullOrEmpty(buyerUuid))
        _gameManager.Antiques.Add(antique); //todo 应该是流入市场，现在这样是流入矿洞
      await Broadcast(updatePlayerMoneyRequest);
    }
  }

  private async Task HandleSpawnPointAsync(Player player)
  {
    var bonus = _gameManager.SelectedMap.SpawnPointCashReward;
    var message =
      new UpdatePlayerMoneyResponse(player.Uuid, bonus, player.Money + bonus)
      {
        LogSegments =
        [
          new LogSegment
          {
            Type = InteractionType.PlayerName,
            Data = player.Uuid
          },
          new LogSegment
          {
            Text = $" 路过了出生点，获得了{bonus}"
          }
        ]
      };
    await Broadcast(message);
  }

  private async Task HandleMineAsync(Player player, NodeModel node)
  {
    var getAntiqueResultResponse =
      new GetAntiqueResultResponse("", "", "", false)
      {
        LogSegments =
        [
          new LogSegment
          {
            Text = "已经没有古玩流通"
          }
        ]
      };

    if (_gameManager.Antiques.Count > 0)
    {
      var randomIndex = Random.Shared.Next(0, _gameManager.Antiques.Count);
      var antique = _gameManager.Antiques[randomIndex];
      var antiqueChangeResponse = new AntiqueChanceResponse(antique.Uuid, player.Uuid, node.Uuid);
      var client = _gameManager.GetClientByPlayerUuid(player.Uuid);
      WeakReferenceMessenger.Default.Send(antiqueChangeResponse, node.Uuid);
      await Broadcast(antiqueChangeResponse);
      var rollDiceResponse = await GetRollDiceAsync(client);
      var isSucceed = rollDiceResponse.DiceValue >= antique.Dice;
      getAntiqueResultResponse =
        new GetAntiqueResultResponse(antique.Uuid, player.Uuid, node.Uuid, isSucceed);
      if (isSucceed)
      {
        getAntiqueResultResponse.LogSegments =
        [
          new LogSegment
          {
            Data = player.Uuid,
            Type = InteractionType.PlayerName
          },
          new LogSegment
          {
            Text = " 获得了 "
          },
          new LogSegment
          {
            Data = antique.Uuid,
            Type = InteractionType.Antique
          }
        ];
      }
      else
      {
        getAntiqueResultResponse.LogSegments =
        [
          new LogSegment
          {
            Data = player.Uuid,
            Type = InteractionType.PlayerName
          },
          new LogSegment
          {
            Text = " 没能获得 "
          },
          new LogSegment
          {
            Data = antique.Uuid,
            Type = InteractionType.Antique
          }
        ];
      }

      WeakReferenceMessenger.Default.Send(getAntiqueResultResponse, node.Uuid);
      await Broadcast(getAntiqueResultResponse); //todo 如果之前打开的地图编辑器，会导致卡死
    }
    else
    {
      var message =
        new UpdatePlayerMoneyResponse(player.Uuid, 200, player.Money + 200)
        {
          LogSegments =
          [
            new LogSegment
            {
              Type = InteractionType.PlayerName,
              Data = player.Uuid
            },
            new LogSegment
            {
              Text = " 路过了矿洞,但已经没有矿物，因此获得了200"
            }
          ]
        };
      await Broadcast(message);
    }
  }

// GameRulerService.cs 内部

  /// <summary>
  /// 核心辅助方法：触发所有符合当前时机的 IStaff 效果
  /// </summary>
  /// <param name="point">当前的时间点（如：路过矿洞、鉴宝时）</param>
  /// <param name="context">上下文数据（包含玩家、骰子点数、古玩信息等）</param>
  private void TriggerGlobalStaffEffects(GameTriggerPoint point, GameContext context)
  {
    foreach (var player in _gameManager.Players)
    {
      var effects = player.GetActiveEffects(point);
      foreach (var effect in effects)
      {
        effect.Execute(context, player);
      }
    }
  }

  private async Task Broadcast<T>(T response) where T : ResponseBase
  {
    await _gameManager.NetServerInstance.Broadcast(response);
    // 此时 T 是具体类型（如 UpdateEstateInfoResponse），Messenger 能正确识别
    WeakReferenceMessenger.Default.Send(response);
    if (response is IHistoryRecord historyRecord)
      WeakReferenceMessenger.Default.Send(historyRecord);
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