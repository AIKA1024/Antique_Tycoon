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

public partial class GameManager : ObservableObject //todo 心跳超时逻辑应该在这里
{
  private readonly NetServer _netServer;
  private readonly NetClient _netClient;
  private readonly MapFileService _mapFileService;

  private readonly ObservableDictionary<string, Player>
    _playersByUuid = []; //不应该存储IConnection，而且使用uuid做减，每次客户端发消息要带上自己的玩家uuid

  private readonly Dictionary<TcpClient, string> _clientToPlayerId = []; //服务器专用
  public Player LocalPlayer { get; private set; }
  [ObservableProperty] public partial Map? SelectedMap { get; set; }
  public INotifyCollectionChangedSynchronizedViewList<Player> Players { get; }
  public int MaxPlayer { get; private set; } = 5;

  public GameManager(NetServer netServer, NetClient netClient, MapFileService mapFileService)
  {
    _netServer = netServer;
    _netClient = netClient;
    _mapFileService = mapFileService;
    _netClient.BroadcastMessageReceived += NetClientOnBroadcastMessageReceived;
    _netServer.ClientDisconnected += NetServerOnClientDisconnected;
    Players = _playersByUuid.ToNotifyCollectionChanged(x => x.Value);
  }

  public void AddLocalPlayer()
  {
    var localPlayer = new Player();
    LocalPlayer = localPlayer;
    _playersByUuid.TryAdd(localPlayer.Uuid, localPlayer);
  }

  public void SetDefaultMap() => SelectedMap = _mapFileService.GetMaps().FirstOrDefault();

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
    await _netServer.BroadcastExcept(updateRoomResponse, client);
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
    await _netServer.Broadcast(startMessage, CancellationToken.None);
  }

  public void ExitRoom()
  {
    var exitRoomRequest = new ExitRoomRequest { PlayerUuid = LocalPlayer.Uuid };
    _ = _netClient.SendRequestAsync(exitRoomRequest);
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
      var data = _netServer.PackMessage(response);
      await client.GetStream().WriteAsync(data);
    }
    else
    {
      _playersByUuid.TryAdd(request.PlayerUuid, request.Player);
      var joinRoomResponse = new JoinRoomResponse
      {
        Id = request.Id,
        Players = Players
      };
      await _netServer.SendResponseAsync(joinRoomResponse, client);

      var updateRoomResponse = new UpdateRoomResponse
      {
        Id = request.Id,
        Players = Players
      };
      _clientToPlayerId.Add(client, request.PlayerUuid);
      await _netServer.BroadcastExcept(updateRoomResponse, client);
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
    await _netServer.BroadcastExcept(updateRoomResponse, client);
  }
}