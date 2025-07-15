using System;
using System.Collections.Generic;
using System.Threading;
using Antique_Tycoon.Models;
using Antique_Tycoon.Net;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels;

public partial class RoomPageViewModel: ViewModelBase
{
  private readonly CancellationTokenSource _cancellationTokenSource;
  public RoomPageViewModel(CancellationTokenSource cts = default)
  {
    _cancellationTokenSource = cts;
    App.Current.Services.GetRequiredService<NetClient>().RoomInfoUpdated += ReceiveUpdateRoomInfo;
    App.Current.Services.GetRequiredService<NetServer>().RoomInfoUpdated += ReceiveUpdateRoomInfo;
  }

  [ObservableProperty] 
  public partial AvaloniaList<Player> Players { get; set; } = [App.Current.Services.GetRequiredService<Player>()];

  private void ReceiveUpdateRoomInfo(IEnumerable<Player> players)
  {
    Players = new AvaloniaList<Player>(players);
  }
  
  public override void OnBacked()
  {
    base.OnBacked();
    _cancellationTokenSource.Cancel();
    App.Current.Services.GetRequiredService<NetClient>().RoomInfoUpdated -= ReceiveUpdateRoomInfo;
    App.Current.Services.GetRequiredService<NetServer>().RoomInfoUpdated -= ReceiveUpdateRoomInfo;
  }
}