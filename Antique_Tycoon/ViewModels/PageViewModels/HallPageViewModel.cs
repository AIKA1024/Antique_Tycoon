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
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using Timer = System.Timers.Timer;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class HallPageViewModel : PageViewModelBase, IDisposable
{
  private readonly Timer _timer = new(2000);
  private bool _disposed;
  private readonly NetClient _client;
  private readonly GameManager _gameManager;

  [ObservableProperty]
  private Bitmap _noMapImage = new(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/No_Map.png")));

  public ObservableCollection<RoomBaseInfo> RoomList { get; } = [];

  public HallPageViewModel(NetClient client, GameManager gameManager)
  {
    _client = client;
    _gameManager = gameManager;
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
    var roomNetInfo = await _client.DiscoverRoomAsync();
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
  private async Task JoinRoom(RoomBaseInfo roomInfo) //todo 职责过重
  {
    var dialogService = App.Current.Services.GetRequiredService<DialogService>();
    var iPEndPoint = new IPEndPoint(IPAddress.Parse(roomInfo.Ip), roomInfo.Port);
    await _client.ConnectServer(iPEndPoint);
    var mapZipPath = Path.Join(App.Current.DownloadMapPath, $"{roomInfo.Hash}.zip");
    var mapDirPath = Path.Join(App.Current.DownloadMapPath, roomInfo.Hash);
    if (!Directory.Exists(mapDirPath))
    {
      var task = DownloadMapAsync(roomInfo.Hash);
      var messageVm = new MessageDialogViewModel
      {
        Title = "提示",
        Message = "地图下载中",
        IsLightDismissEnabled = false,
        IsShowConfirmButton = false
      };
      var result = await dialogService.ShowDialogAsync(messageVm,task);
      messageVm.CloseDialog();
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
      ZipFile.ExtractToDirectory(mapZipPath, mapDirPath);
      File.Delete(mapZipPath);
    }

    var response = await JoinRoomAsync();
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

    _gameManager.LocalPlayer.IsHomeowner = false;
    var mapFileService = App.Current.Services.GetRequiredService<MapFileService>();
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(
      new RoomPageViewModel(mapFileService.LoadMap(mapDirPath),
        App.Current.Services.GetRequiredService<NetClient>(),
        App.Current.Services.GetRequiredService<NetServer>(),
        _gameManager));
  }

  private async Task<JoinRoomResponse> JoinRoomAsync(CancellationToken cancellation = default)
  {
    var joinRoomRequest = new JoinRoomRequest
    {
      Player = _gameManager.LocalPlayer
    };
    return (JoinRoomResponse)await _client.SendRequestAsync(joinRoomRequest, cancellation);
  }

  private async Task<DownloadMapResponse> DownloadMapAsync(string hash, CancellationToken cancellation = default)
  {
    var downloadMapRequest = new DownloadMapRequest(hash);
    return (DownloadMapResponse)await _client.SendRequestAsync(downloadMapRequest, cancellation);
  }

  public void Dispose()
  {
    if (_disposed) return;
    NoMapImage.Dispose();
    _disposed = true;
    _timer.Stop();
  }
}