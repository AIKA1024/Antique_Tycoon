using System;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Node;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;

namespace Antique_Tycoon.Services;

/// <summary>
/// 游戏规则核心服务，封装所有玩法逻辑
/// </summary>
public partial class GameRuleService : ObservableObject
{
  private readonly GameManager _gameManager;
  private readonly MapFileService _mapFileService;
  private Player _currentTurnPlayer; // 当前回合玩家

    // 游戏状态（可绑定到UI）
    [ObservableProperty]
    public partial int CurrentRound { get; set; }

    [ObservableProperty]
    public partial bool IsGameOver { get; set; }

    [ObservableProperty]
    public partial Player? Winner { get; set; }

    public GameRuleService(GameManager gameManager, MapFileService mapFileService)
  {
    _gameManager = gameManager;
    _mapFileService = mapFileService;
    // 订阅游戏启动事件（从 GameManager 或 ViewModel 发送）
    WeakReferenceMessenger.Default.Register<GameStartMessage>(this, (_, _) => StartGameAsync());
  }
  
  /// <summary>
  /// 启动游戏（初始化回合、资源）
  /// </summary>
  public void StartGameAsync()
  {
    CurrentRound = 1;
    IsGameOver = false;
    Winner = null;
    // 初始化所有玩家现金（从地图配置读取初始金额）
    foreach (var player in _gameManager.Players)
      player.Money = _gameManager.SelectedMap!.StartingCash;
    // 随机指定第一个回合玩家
    _currentTurnPlayer = _gameManager.Players[Random.Shared.Next(_gameManager.Players.Count)];
    // 通知UI：回合开始
    WeakReferenceMessenger.Default.Send(new TurnStartMessage(_currentTurnPlayer));
  }
  
  /// <summary>
  /// 玩家购买地产
  /// </summary>
  /// <param name="player">购买玩家</param>
  /// <param name="estate">目标地产</param>
  /// <returns>购买结果（成功/失败原因）</returns>
  public async Task<(bool Success, string Message)> BuyEstateAsync(Player player, Estate estate)
  {
    // 规则1：只能购买无主地产
    if (estate.Owner != null)
      return (false, "该地产已被其他玩家拥有！");
        
    // 规则2：玩家现金需 ≥ 地产价值
    if (player.Money < estate.Value)
      return (false, "现金不足，无法购买！");
        
    // 执行购买逻辑
    player.Money -= estate.Value;
    estate.Owner = player;
        
    // 通知UI：地产购买成功
    WeakReferenceMessenger.Default.Send(new EstateBoughtMessage(player, estate));
        
    // 联机场景：同步地产归属到其他客户端
    if (!player.IsHomeowner)
    {
      await _gameManager.NetClientInstance.SendRequestAsync(new UpdateEstateOwnerRequest
      {
        PlayerUuid = player.Uuid,
        EstateUuid = estate.Uuid
      });
    }
    else
    {
      // 房主广播更新
      await _gameManager.NetServerInstance.Broadcast(new UpdateEstateOwnerResponse
      {
        EstateUuid = estate.Uuid,
        OwnerUuid = player.Uuid
      });
    }

    return (true, $"成功购买「{estate.Title}」！");
  }
}