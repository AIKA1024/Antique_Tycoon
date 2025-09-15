using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Net;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class RoomPageViewModel : PageViewModelBase
{
  private readonly CancellationTokenSource? _cancellationTokenSource; // 如果是加入别人的房间，这个就会是null

  public Player SelfPlayer { get; set; } = App.Current.Services.GetRequiredService<Player>();
  private Map SelectedMap { get; }

  public RoomPageViewModel(Map map, CancellationTokenSource? cts = null)
  {
    SelectedMap = map;
    _cancellationTokenSource = cts;
    App.Current.Services.GetRequiredService<NetClient>().GameStarted += OnGameStarted;
    App.Current.Services.GetRequiredService<NetClient>().RoomInfoUpdated += ReceiveUpdateRoomInfo;
    App.Current.Services.GetRequiredService<NetServer>().RoomInfoUpdated += ReceiveUpdateRoomInfo;
  }

  private void OnGameStarted(StartGameResponse _)
  {
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new GamePageViewModel(SelectedMap));
  }

  [ObservableProperty]
  public partial ObservableCollection<Player> Players { get; set; } =
    [App.Current.Services.GetRequiredService<Player>()];

  private void ReceiveUpdateRoomInfo(IEnumerable<Player> players)
  {
    Players = new ObservableCollection<Player>(players);
  }

  [RelayCommand]
  private void StartGame()
  {
    App.Current.Services.GetRequiredService<NetServer>().StartGame();
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new GamePageViewModel(SelectedMap));
  }

  public override void OnBacked()
  {
    base.OnBacked();
    if (!App.Current.Services.GetRequiredService<Player>().IsHomeowner)
      App.Current.Services.GetRequiredService<NetClient>().ExitRoom();
    App.Current.Services.GetRequiredService<NetClient>().RoomInfoUpdated -= ReceiveUpdateRoomInfo;
    App.Current.Services.GetRequiredService<NetServer>().RoomInfoUpdated -= ReceiveUpdateRoomInfo;
    _cancellationTokenSource?.Cancel();
  }
}