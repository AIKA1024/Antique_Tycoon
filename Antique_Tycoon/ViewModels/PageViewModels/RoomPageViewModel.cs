using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Net;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Player = Antique_Tycoon.Models.Player;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class RoomPageViewModel : PageViewModelBase
{
  private readonly CancellationTokenSource? _cancellationTokenSource; // 如果是加入别人的房间，这个就会是null

  private Map SelectedMap { get; }
  private readonly GameManager _gameManager;
  [ObservableProperty] public partial IList<Player> Players { get; set; }
  public Player LocalPlayer { get; }


  public RoomPageViewModel(Map map, GameManager gameManager,
    CancellationTokenSource? cts = null)
  {
    SelectedMap = map;
    _cancellationTokenSource = cts;
    _gameManager = gameManager;
    Players = _gameManager.Players;
    LocalPlayer = _gameManager.LocalPlayer;
    _gameManager.Players.CollectionChanged += OnPlayersChanged;
    WeakReferenceMessenger.Default.Register<GameStartMessage>(this, (_, _) => App.Current.Services
      .GetRequiredService<NavigationService>().Navigation(new GamePageViewModel(SelectedMap)));
    App.Current.Services.GetRequiredService<GameRuleService>();// 启动gameRule
  }


  private void OnPlayersChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    // 简单地重新赋值，会触发 ViewModel 的 Players 属性的更新，并通知 UI
    Players = _gameManager.Players;
  }
  

  [RelayCommand]
  private async Task StartGame()
  {
    await _gameManager.StartGameAsync();
  }

  public override void OnBacked()
  {
    base.OnBacked();
    if (!LocalPlayer.IsRoomOwner)
      _gameManager.ExitRoom();
    _cancellationTokenSource?.Cancel();
  }

  public override void OnNavigatingFrom()
  {
    base.OnNavigatingFrom();
    _gameManager.Players.CollectionChanged -= OnPlayersChanged;
  }
}