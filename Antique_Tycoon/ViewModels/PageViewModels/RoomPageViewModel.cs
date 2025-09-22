using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Net;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using ObservableCollections;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class RoomPageViewModel : PageViewModelBase
{
  private readonly CancellationTokenSource? _cancellationTokenSource; // 如果是加入别人的房间，这个就会是null

  private Map SelectedMap { get; }
  private readonly NetClient _client;
  private readonly GameManager _gameManager;
  [ObservableProperty] public partial INotifyCollectionChangedSynchronizedViewList<Player> Players { get; set; }
  public Player LocalPlayer { get; }


  public RoomPageViewModel(Map map, NetClient netClient, NetServer netServer, GameManager gameManager,
    CancellationTokenSource? cts = null)
  {
    SelectedMap = map;
    _cancellationTokenSource = cts;
    _client = netClient;
    _gameManager = gameManager;
    _client.BroadcastMessageReceived += ClientOnBroadcastMessageReceived;
    Players = _gameManager.Players;
    LocalPlayer = _gameManager.LocalPlayer;
  }

  private void ClientOnBroadcastMessageReceived(ITcpMessage message)
  {
    switch (message)
    {
      case StartGameResponse:
        App.Current.Services.GetRequiredService<NavigationService>().Navigation(new GamePageViewModel(SelectedMap));
        break;
    }
  }

  [RelayCommand]
  private async Task StartGame()
  {
    await _gameManager.StartGameAsync();
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new GamePageViewModel(SelectedMap));
  }

  public override void OnBacked()
  {
    base.OnBacked();
    if (!LocalPlayer.IsHomeowner)
      _gameManager.ExitRoom();
    _cancellationTokenSource?.Cancel();
  }
}