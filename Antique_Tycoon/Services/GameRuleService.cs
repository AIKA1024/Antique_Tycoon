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
using Player = Antique_Tycoon.Models.Player;

namespace Antique_Tycoon.Services;

/// <summary>
/// 游戏规则核心服务，封装所有玩法逻辑
/// </summary>
public partial class GameRuleService : ObservableObject
{
    private readonly RoleStrategyFactory _strategyFactory; // 应该在回合结束时播放音效
    private readonly GameManager _gameManager;
    private readonly MapFileService _mapFileService;
    private int _currentTurnPlayerIndex;
    public Player CurrentTurnPlayer => _gameManager.Players[_currentTurnPlayerIndex]; // 当前回合玩家

    // 游戏状态（可绑定到UI）
    [ObservableProperty] public partial int CurrentRound { get; set; }

    [ObservableProperty] public partial bool IsGameOver { get; set; }

    [ObservableProperty] public partial Player? Winner { get; set; }

    public GameRuleService(GameManager gameManager, MapFileService mapFileService, LibVLC libVlc,
        RoleStrategyFactory strategyFactory)
    {
        _gameManager = gameManager;
        _mapFileService = mapFileService;
        _strategyFactory = strategyFactory;
        var sfxPlayer = new MediaPlayer(libVlc);
        var turnStartSfx = new Media(libVlc, "Assets/SFX/GameStates/LevelUp.ogg");
        WeakReferenceMessenger.Default.Register<GameStartMessage>(this, async (_, _) => await StartGameAsync());
        WeakReferenceMessenger.Default.Register<TurnStartMessage>(this, (_, message) =>
        {
            if (message.Value == _gameManager.LocalPlayer)
                sfxPlayer.Play(turnStartSfx);
        });
        WeakReferenceMessenger.Default.Register<InitGameMessage>(this,
            (_, message) => _currentTurnPlayerIndex = message.CurrentTurnPlayerIndex);
    }

    private async Task NotifyCurrentPlayerTurnStart() //todo 到下一个回合时，这个方法没有调用（回合逻辑没写，每回合应该计算各种逻辑，现在回合还不会结束）
    {
        var turnStartResponse = new TurnStartResponse { Player = CurrentTurnPlayer };
        await _gameManager.NetServerInstance.Broadcast(turnStartResponse);
        _currentTurnPlayerIndex = (_currentTurnPlayerIndex + 1) % _gameManager.Players.Count;
        WeakReferenceMessenger.Default.Send(new TurnStartMessage(CurrentTurnPlayer));
    }

    /// <summary>
    /// 启动游戏（初始化回合、资源）
    /// </summary>
    public async Task StartGameAsync()
    {
        if (!_gameManager.LocalPlayer.IsRoomOwner)
            return;
        CurrentRound = 1;
        IsGameOver = false;
        Winner = null;
        // 初始化所有玩家（从地图配置读取初始金额）
        foreach (var player in _gameManager.Players)
        {
            player.Money = _gameManager.SelectedMap!.StartingCash;
            player.CurrentNodeUuId = _gameManager.SelectedMap!.SpawnNodeUuid;
        }

        _currentTurnPlayerIndex = Random.Shared.Next(_gameManager.Players.Count);
        // _currentTurnPlayerIndex = 1;
        await _gameManager.NetServerInstance.Broadcast(new InitGameMessageResponse(
            _gameManager.Players,
            _currentTurnPlayerIndex
        ));
        await Task.Delay(1000); // 等待各监听事件绑定完成
        await NotifyCurrentPlayerTurnStart();
    }


    public async Task RollDiceAsync()
    {
        var selfPlayer = _gameManager.LocalPlayer;
        if (selfPlayer.IsRoomOwner)
        {
            if (selfPlayer != CurrentTurnPlayer)
            {
                WeakReferenceMessenger.Default.Send(new RollDiceMessage(selfPlayer.Uuid, 0, false));
                return;
            }

            int rollValue = Random.Shared.Next(1, 7);
            await _gameManager.NetServerInstance.Broadcast(new RollDiceResponse("", selfPlayer.Uuid, rollValue));
            WeakReferenceMessenger.Default.Send(new RollDiceMessage(selfPlayer.Uuid, rollValue));
        }
        else
            await _gameManager.NetClientInstance.SendRequestAsync(new RollDiceRequest());
    }

    public async Task PlayerMove(string destinationNodeUuid)
    {
        if (_gameManager.LocalPlayer.IsRoomOwner)
        {
            await _gameManager.NetServerInstance.Broadcast(new PlayerMoveResponse(_gameManager.LocalPlayer.Uuid,
                destinationNodeUuid));
            WeakReferenceMessenger.Default.Send(new PlayerMoveMessage(_gameManager.LocalPlayer.Uuid,
                destinationNodeUuid));
        }
        else
            await _gameManager.NetClientInstance.SendRequestAsync(new PlayerMoveRequest(_gameManager.LocalPlayer.Uuid,
                destinationNodeUuid));
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
        if (!player.IsRoomOwner)
        {
            await _gameManager.NetClientInstance.SendRequestAsync(
                new UpdateEstateInfoRequest(player.Uuid, estate.Uuid));
        }
        else
        {
            // 房主广播更新
            await _gameManager.NetServerInstance.Broadcast(new UpdateEstateInfoResponse(estate.Uuid, player.Uuid));
        }

        return (true, $"成功购买「{estate.Title}」！");
    }
}