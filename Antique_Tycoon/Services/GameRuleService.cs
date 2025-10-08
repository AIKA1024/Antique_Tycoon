using System;
using System.Net.Sockets;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Node;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LibVLCSharp.Shared;

namespace Antique_Tycoon.Services;

/// <summary>
/// 游戏规则核心服务，封装所有玩法逻辑
/// </summary>
public partial class GameRuleService : ObservableObject
{
  private MediaPlayer _sfxPlayer;
  private static Media _turnStartSFX;
  private readonly GameManager _gameManager;
  private readonly MapFileService _mapFileService;
  private int _currentTurnPlayerIndex;
  private Player CurrentTurnPlayer => _gameManager.Players[_currentTurnPlayerIndex]; // 当前回合玩家

  // 游戏状态（可绑定到UI）
  [ObservableProperty] public partial int CurrentRound { get; set; }

  [ObservableProperty] public partial bool IsGameOver { get; set; }

  [ObservableProperty] public partial Player? Winner { get; set; }

  public GameRuleService(GameManager gameManager, MapFileService mapFileService, LibVLC libVlc)
  {
    _gameManager = gameManager;
    _mapFileService = mapFileService;
    _sfxPlayer = new MediaPlayer(libVlc);
    _turnStartSFX = new Media(libVlc, "Assets/SFX/LevelUp.ogg");
    WeakReferenceMessenger.Default.Register<GameStartMessage>(this, async (_, _) => await StartGameAsync());
    WeakReferenceMessenger.Default.Register<TurnStartMessage>(this, (_, message) =>
    {
      if (message.Value == _gameManager.LocalPlayer)
        _sfxPlayer.Play(_turnStartSFX);
    });
  }

  /// <summary>
  /// 启动游戏（初始化回合、资源）
  /// </summary>
  public async Task StartGameAsync()
  {
    CurrentRound = 1;
    IsGameOver = false;
    Winner = null;
    // 初始化所有玩家现金（从地图配置读取初始金额）
    foreach (var player in _gameManager.Players)
      player.Money = _gameManager.SelectedMap!.StartingCash;
    _currentTurnPlayerIndex = Random.Shared.Next(_gameManager.Players.Count);
    await NotifyCurrentPlayerTurnStart();
  }

  private async Task NotifyCurrentPlayerTurnStart()
  {
    var turnStartResponse = new TurnStartResponse { Player = CurrentTurnPlayer };
    await _gameManager.NetServerInstance.Broadcast(turnStartResponse);
    _currentTurnPlayerIndex = (_currentTurnPlayerIndex + 1) % _gameManager.Players.Count;
    WeakReferenceMessenger.Default.Send(new TurnStartMessage(CurrentTurnPlayer));
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