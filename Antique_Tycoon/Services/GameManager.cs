using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Net;
using CommunityToolkit.Mvvm.ComponentModel;
using ObservableCollections;

namespace Antique_Tycoon.Services;

/// <summary>
/// 游戏数据核心服务，封装基本业务操作
/// </summary>
public partial class GameManager : ObservableObject //todo 心跳超时逻辑应该在这里
{
  private readonly Lazy<NetServer> _netServerLazy;
  private readonly Lazy<NetClient> _netClientLazy;
  
  private readonly MapFileService _mapFileService;

  private readonly ObservableDictionary<string, Player> _playersByUuid = [];

  private readonly Dictionary<TcpClient, string> _clientToPlayerId = []; //服务器专用
  public NetServer NetServerInstance => _netServerLazy.Value;
  public NetClient NetClientInstance => _netClientLazy.Value;
  public Player LocalPlayer { get; set; }
  [ObservableProperty] public partial Map? SelectedMap { get; set; }
  public NotifyCollectionChangedSynchronizedViewList<Player> Players { get; }
  public int MaxPlayer { get; private set; } = 5;

  public GameManager(Lazy<NetServer> netServerLazy, Lazy<NetClient> netClientLazy, MapFileService mapFileService)
  {
    _netServerLazy = netServerLazy;
    _netClientLazy = netClientLazy;
    _mapFileService = mapFileService;
    Players = _playersByUuid.ToNotifyCollectionChanged(x => x.Value);
  }

  public void AddLocalPlayer()
  {
    var localPlayer = new Player{IsHomeowner = true};
    LocalPlayer = localPlayer;
    _playersByUuid.TryAdd(localPlayer.Uuid, localPlayer);
  }

  public void SetDefaultMap() => SelectedMap = _mapFileService.GetMaps().FirstOrDefault();

  public void ListenEvent()
  {
    NetClientInstance.BroadcastMessageReceived += NetClientOnBroadcastMessageReceived;
    NetServerInstance.ClientDisconnected += NetServerOnClientDisconnected;
  }

  private async Task NetServerOnClientDisconnected(TcpClient client)
  {
    var playerUuid = _clientToPlayerId[client];
    _clientToPlayerId.Remove(client);
    _playersByUuid.Remove(playerUuid);
    var updateRoomResponse = new UpdateRoomResponse
    {
      Id = Guid.NewGuid().ToString(),
      Players = Players
    };
    await NetServerInstance.BroadcastExcept(updateRoomResponse, client);
  }

  private void NetClientOnBroadcastMessageReceived(ITcpMessage message)
  {
    switch (message)
    {
      case UpdateRoomResponse updateRoomResponse:
        _playersByUuid.Clear();
        foreach (var player in updateRoomResponse.Players)
          _playersByUuid[player.Uuid] = player;
        break;
    }
  }

  public async Task StartGameAsync()
  {
    var startMessage = new StartGameResponse();
    await NetServerInstance.Broadcast(startMessage, CancellationToken.None);
  }

  public void ExitRoom()
  {
    var exitRoomRequest = new ExitRoomRequest { PlayerUuid = LocalPlayer.Uuid };
    _ = NetClientInstance.SendRequestAsync(exitRoomRequest);
  }

  public async Task ReceiveJoinRoomRequest(JoinRoomRequest request, TcpClient client)
  {
    if (_playersByUuid.Count >= MaxPlayer)
    {
      var response = new JoinRoomResponse
      {
        Id = request.Id,
        Message = "房间已满",
        ResponseStatus = RequestResult.Reject
      };
      var data = NetServerInstance.PackMessage(response);
      await client.GetStream().WriteAsync(data);
    }
    else
    {
      _playersByUuid.TryAdd(request.PlayerUuid, request.Player);
      var joinRoomResponse = new JoinRoomResponse
      {
        Id = request.Id,
        // Players = Players
      };
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

  public async Task ReceiveExitRoomRequest(ExitRoomRequest request, TcpClient client)
  {
    _playersByUuid.Remove(request.PlayerUuid);
    var updateRoomResponse = new UpdateRoomResponse
    {
      Id = request.Id,
      Players = Players
    };
    await NetServerInstance.BroadcastExcept(updateRoomResponse, client);
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