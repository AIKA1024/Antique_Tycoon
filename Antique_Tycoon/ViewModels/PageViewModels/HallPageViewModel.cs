using System;
using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Net;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class HallPageViewModel : PageViewModelBase, IDisposable
{
  private readonly Timer _timer = new(2000);
  private bool _disposed;

  [ObservableProperty]
  private Bitmap _noMapImage = new(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/No_Map.png")));

  public ObservableCollection<RoomBaseInfo> RoomList { get; } = [];

  public HallPageViewModel()
  {
    _timer.Elapsed += async (s, e) => { await UpdateRoomList(s, e); };
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
  private async Task JoinRoom(RoomBaseInfo roomInfo)//todo 职责过重
  {
    var client = App.Current.Services.GetRequiredService<NetClient>();
    var dialogService = App.Current.Services.GetRequiredService<DialogService>();
    var iPEndPoint = new IPEndPoint(IPAddress.Parse(roomInfo.Ip), roomInfo.Port);
    await client.ConnectServer(iPEndPoint);
    var mapPath = Path.Join(App.Current.DownloadMapPath, $"{roomInfo.Hash}.zip");
    var mapDirPath = Path.Join(App.Current.DownloadMapPath, roomInfo.Hash);
    if (!Directory.Exists(mapDirPath))
    {
      var result = await client.DownloadMapAsync();
      if (result.ResponseStatus != RequestResult.Success)
      {
        await dialogService.ShowDialogAsync(
          new MessageDialogViewModel
          {
            Title = "提示",
            Message = "地图下载失败"
          });
        return;
      }
      
      await dialogService.ShowDialogAsync(
        new MessageDialogViewModel
        {
          Title = "提示",
          Message = "下载成功"
        });
      ZipFile.ExtractToDirectory(mapPath,mapDirPath);
      File.Delete(mapPath);
    }

    
    var response = await client.JoinRoomAsync();
    if (response.ResponseStatus != RequestResult.Success)
    {
      await dialogService.ShowDialogAsync(
        new MessageDialogViewModel
        {
          Title = "提示",
          Message = response.Message
        });
      return;
    }

    App.Current.Services.GetRequiredService<Player>().IsHomeowner = false;
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