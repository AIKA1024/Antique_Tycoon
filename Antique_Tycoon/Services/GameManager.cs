using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Behaviors;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Node;
using Antique_Tycoon.Net;
using Antique_Tycoon.ViewModels.DialogViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using LibVLCSharp.Shared;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;
using Player = Antique_Tycoon.Models.Player;

namespace Antique_Tycoon.Services;

/// <summary>
/// 游戏数据核心服务，封装基本业务操作
/// </summary>
public partial class GameManager : ObservableObject //todo 心跳超时逻辑应该在这里
{
  private readonly Lazy<NetServer> _netServerLazy; //如果依赖注入后，需要NetServer、NetClient的同时需要GameManager，只需要拿GameManager就行了

  private readonly Lazy<NetClient> _netClientLazy;

  // private readonly GameRuleService _gameRuleService;
  private readonly LibVLC _libVlc;
  private readonly RoleStrategyFactory _strategyFactory;
  private readonly AnimationManager _animationManager;

  private readonly MapFileService _mapFileService;

  private readonly ObservableDictionary<string, Player> _playersByUuid = [];

  private string _localPlayerUuid;

  private readonly Dictionary<TcpClient, string> _clientToPlayerId = []; //服务器专用
  public readonly SemaphoreSlim GameActionLock = new SemaphoreSlim(1, 1);
  public NetServer NetServerInstance => _netServerLazy.Value;
  public NetClient NetClientInstance => _netClientLazy.Value;
  public Player LocalPlayer => _playersByUuid[_localPlayerUuid];
  [ObservableProperty] public partial Map? SelectedMap { get; set; }
  public string RoomOwnerUuid { get; set; } = "";
  public bool IsRoomOwner => RoomOwnerUuid == LocalPlayer.Uuid;
  public NotifyCollectionChangedSynchronizedViewList<Player> Players { get; }
  public int CurrentTurnPlayerIndex { get; set; }
  [ObservableProperty] public partial int CurrentRound { get; set; }

  [ObservableProperty] public partial bool IsGameOver { get; set; }

  [ObservableProperty] public partial Player? Winner { get; set; }
  public Player CurrentTurnPlayer => Players[CurrentTurnPlayerIndex]; // 当前回合玩家 

  public GameManager(Lazy<NetServer> netServerLazy, Lazy<NetClient> netClientLazy, MapFileService mapFileService,
    LibVLC libVlc, RoleStrategyFactory strategyFactory, AnimationManager animationManager)
  {
    _netServerLazy = netServerLazy;
    _netClientLazy = netClientLazy;
    _mapFileService = mapFileService;
    _libVlc = libVlc;
    _strategyFactory = strategyFactory;
    _animationManager = animationManager;
    var sfxPlayer = new MediaPlayer(libVlc);
    var turnStartSfx = new Media(libVlc, "Assets/SFX/GameStates/LevelUp.ogg");
    WeakReferenceMessenger.Default.Register<TurnStartResponse>(this, (_, message) =>
    {
      if (message.PlayerUuid == LocalPlayer.Uuid)
        sfxPlayer.Play(turnStartSfx);
    });
    Players = _playersByUuid.ToNotifyCollectionChanged(x => x.Value);

    WeakReferenceMessenger.Default.Register<UpdateRoomResponse>(this, ReceiveUpdateRoomResponse);
    WeakReferenceMessenger.Default.Register<ExitRoomResponse>(this,
      (_, request) => _playersByUuid.Remove(request.PlayerUuid));
    //因为要更新其他玩家的信息，所以也要监听这个消息
    WeakReferenceMessenger.Default.Register<PlayerMoveResponse>(this, ReceivePlayerMoveResponse);
  }

  public void Initialize()
  {
    SetupLocalPlayer();
    SetDefaultMap();
    _netServerLazy.Value.ClientDisConnected += ClientDisConnected;
  }

  /// <summary>
  /// 服务器通过心跳发现玩家掉线
  /// </summary>
  /// <param name="client">掉线客户端</param>
  private async void ClientDisConnected(TcpClient client)
  {
    var playerUuid = _clientToPlayerId[client];
    _clientToPlayerId.Remove(client);
    _playersByUuid.Remove(playerUuid);
    var exitRoomResponse = new ExitRoomResponse(playerUuid);
    await NetServerInstance.BroadcastExcept(exitRoomResponse, client);
    WeakReferenceMessenger.Default.Send(exitRoomResponse);
  }

  private void ReceiveUpdateRoomResponse(object _, UpdateRoomResponse message)
  {
    _playersByUuid.Clear();
    foreach (var player in message.Players)
      _playersByUuid[player.Uuid] = player;
  }

  private async void ReceivePlayerMoveResponse(object recipient, PlayerMoveResponse message)
  {
    Player player = GetPlayerByUuid(message.PlayerUuid);
    string playerCurrentNodeUuid = player.CurrentNodeUuId;
    NodeModel currentModelmodel = (NodeModel)SelectedMap.EntitiesDict[playerCurrentNodeUuid];
    NodeModel destinationModelmodel = (NodeModel)SelectedMap.EntitiesDict[message.Path[^1]];
    currentModelmodel.PlayersHere.Remove(player);
    await _animationManager.StartPlayerMoveAnimation(player, message.Path, message.Id);
    destinationModelmodel.PlayersHere.Add(player);
    player.CurrentNodeUuId = destinationModelmodel.Uuid;
  }

  private void SetupLocalPlayer()
  {
    var localPlayer = new Player();
    _localPlayerUuid = localPlayer.Uuid;
    RoomOwnerUuid = _localPlayerUuid;
    _playersByUuid.TryAdd(localPlayer.Uuid, localPlayer);
  }

  private void SetDefaultMap() => SelectedMap = _mapFileService.GetMaps().FirstOrDefault();

  public TcpClient? GetClientByPlayerUuid(string uuid) =>
    _clientToPlayerId.FirstOrDefault(variable => variable.Value == uuid).Key;

  public string GetPlayerUuidByTcpClient(TcpClient client)
  {
    if (!IsRoomOwner)
      throw new InvalidOperationException("客户端不能调用此方法");
    return _clientToPlayerId[client];
  }

  public Player GetPlayerByUuid(string uuid) => _playersByUuid[uuid];
  

  public async Task StartGameAsync()
  {
    var startMessage = new StartGameResponse();
    await NetServerInstance.Broadcast(startMessage, CancellationToken.None);
    WeakReferenceMessenger.Default.Send(startMessage);
    if (!IsRoomOwner)
      return;
    CurrentRound = 1;
    IsGameOver = false;
    Winner = null;
    // 初始化所有玩家（从地图配置读取初始金额）
    foreach (var player in Players)
    {
      player.Money = SelectedMap!.StartingCash;
      player.CurrentNodeUuId = SelectedMap!.SpawnNodeUuid;
    }

    CurrentTurnPlayerIndex = Random.Shared.Next(Players.Count);
    // CurrentTurnPlayerIndex = 1;
    await NetServerInstance.Broadcast(new InitGameResponse(
      Players,
      CurrentTurnPlayerIndex
    ));
    
    //通知游戏开始
    CurrentTurnPlayerIndex = (CurrentTurnPlayerIndex + 1) % Players.Count;
    var turnStartResponse = new TurnStartResponse { PlayerUuid = CurrentTurnPlayer.Uuid };
    await NetServerInstance.Broadcast(turnStartResponse);
    WeakReferenceMessenger.Default.Send(turnStartResponse);
    // await Task.Yield(); // 等待各监听事件绑定完成
  }

  private async Task PlayerMove(List<string> path)
  {
    if (IsRoomOwner)
    {
      var response = new PlayerMoveResponse(LocalPlayer.Uuid, path);
      await NetServerInstance.Broadcast(response);
      WeakReferenceMessenger.Default.Send(response);
    }
    else
      await NetClientInstance.SendRequestAsync(new PlayerMoveRequest(LocalPlayer.Uuid, path));
  }


  public void ExitRoom()
  {
    var exitRoomRequest = new ExitRoomRequest { PlayerUuid = LocalPlayer.Uuid };
    _ = NetClientInstance.SendRequestAsync(exitRoomRequest);
  }

  public async Task ReceiveJoinRoomRequest(JoinRoomRequest request, TcpClient client) //todo 不知道要不要把逻辑移动到JoinRoomHandler里
  {
    if (_playersByUuid.Count >= SelectedMap.MaxPlayer)
    {
      var response = new JoinRoomResponse(LocalPlayer.Uuid)
      {
        Id = request.Id,
        Message = "房间已满",
        ResponseStatus = RequestResult.Reject
      };
      await NetServerInstance.SendResponseAsync(response, client);
    }
    else
    {
      _playersByUuid.TryAdd(request.PlayerUuid, request.Player);
      var joinRoomResponse = new JoinRoomResponse(LocalPlayer.Uuid) { Id = request.Id };
      await NetServerInstance.SendResponseAsync(joinRoomResponse, client);

      var updateRoomResponse = new UpdateRoomResponse
      {
        Id = request.Id,
        Players = Players
      };
      _clientToPlayerId.Add(client, request.PlayerUuid);
      await NetServerInstance.Broadcast(updateRoomResponse);
    }
  }

  public async Task DownloadMap(DownloadMapRequest request, TcpClient client)
  {
    if (_mapFileService.GetMapByHash(request.Hash) is { } map)
    {
      await NetServerInstance.SendFileAsync(_mapFileService.GetMapFileStream(map),
        request.Id, $"{_mapFileService.GetMapFileHash(map)}.zip", TcpMessageType.DownloadMapResponse,
        client);
    }
    else
      await NetServerInstance.SendResponseAsync(
        new DownloadMapResponse { Id = request.Id, ResponseStatus = RequestResult.Error }, client);
  }
}