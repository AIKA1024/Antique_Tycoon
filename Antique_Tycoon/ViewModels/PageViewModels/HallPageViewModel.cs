using System;
using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Net;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using Antique_Tycoon.Models;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class HallPageViewModel : PageViewModelBase,IDisposable
{
  private readonly Timer _timer = new(2000);
  private bool _disposed;
  [ObservableProperty] private Bitmap _noMapImage = new(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/No_Map.png")));
  public ObservableCollection<RoomBaseInfo> RoomList { get; } = [];
  public HallPageViewModel()
  {
    _timer.Elapsed += async (s, e) =>
    {
      await UpdateRoomList(s, e);
    };
  }

  public override void OnNavigatedTo()
  {
    base.OnNavigatedTo();
    _timer.Start();
  }

  public override void OnNavigatingFrom()
  {
    base.OnNavigatingFrom();
    _timer.Stop();
  }

  private async Task UpdateRoomList(object? sender, ElapsedEventArgs e)
  {
    var roomNetInfo = await App.Current.Services.GetRequiredService<NetClient>().DiscoverRoomAsync();
    if (RoomList.Any(r => Equals(r.Ip, roomNetInfo.Ip)))
      return;
    RoomList.Add(roomNetInfo);
  }

  [RelayCommand]
  private void NavigateToCreateRoomPage()
  {
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new CreateRoomPageViewModel());
  }

  [RelayCommand]
  private async Task JoinRoom(RoomBaseInfo roomInfo)
  {
    var iPEndPoint = new IPEndPoint(IPAddress.Parse(roomInfo.Ip), roomInfo.Port);
    await App.Current.Services.GetRequiredService<NetClient>().ConnectServer(iPEndPoint);
    var response = await App.Current.Services.GetRequiredService<NetClient>().JoinRoomAsync();
    if (response.Players.Count == 0)
    {
      return;//todo 要显示提示
    }

    App.Current.Services.GetRequiredService<Player>().IsHomeowner = false;//todo 要下载地图
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new RoomPageViewModel
    {
      Players = response.Players
    });
  }

  public void Dispose()
  {
    if (_disposed) return;
    NoMapImage.Dispose();
    _disposed = true;
    _timer.Stop();
  }
}