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
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Net.Udp;
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
  private readonly GameManager _gameManager = App.Current.Services.GetRequiredService<GameManager>();
  private readonly DialogService _dialogService = App.Current.Services.GetRequiredService<DialogService>();
  private readonly MapFileService _mapFileService = App.Current.Services.GetRequiredService<MapFileService>();


  [ObservableProperty]
  private Bitmap _noMapImage = new(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/No_Map.png")));

  public ObservableCollection<ServiceInfo> RoomList { get; } = [];

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
    var roomNetInfo = await _gameManager.NetClientInstance.DiscoverRoomAsync();
    if (RoomList.Any(r => Equals(r.Address, roomNetInfo.Address)))
      return;
    RoomList.Add(roomNetInfo);
  }

  [RelayCommand]
  private async Task AddService()
  {
    var serviceInfo = await _dialogService.ShowDialogAsync(new AddServiceDialogViewModel());
    if (serviceInfo == null)
      return;
    RoomList.Add(serviceInfo);
  }

  [RelayCommand]
  private void NavigateToCreateRoomPage()
  {
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new CreateRoomPageViewModel());
  }

  [RelayCommand]
  private async Task JoinRoom(ServiceInfo serviceInfo) //todo 职责过重
  {
    try
    {
      // 自动识别：IP 或 域名
      if (!IPAddress.TryParse(serviceInfo.Address, out var ipAddress))
      {
        // 域名解析
        var addresses = await Dns.GetHostAddressesAsync(serviceInfo.Address);
        ipAddress = addresses.FirstOrDefault(ip => ip.AddressFamily == AddressFamily.InterNetwork);
      }

      if (ipAddress == null)
      {
        await _dialogService.ShowDialogAsync(new MessageDialogViewModel
        {
          Title = "错误",
          Message = "无法解析服务器地址"
        });
        return;
      }

      var ipEndPoint = new IPEndPoint(ipAddress, serviceInfo.Port);
      var connectTask = _gameManager.NetClientInstance.ConnectServer(ipEndPoint);

      // 弹窗等待连接
      await _dialogService.ShowDialogAsync(
        new MessageDialogViewModel
        {
          Title = "提示",
          Message = "正在连接服务器...",
          IsShowConfirmButton = false,
          IsLightDismissEnabled = false
        },
        connectTask);
    }
    catch (Exception ex)
    {
      await _dialogService.ShowDialogAsync(new MessageDialogViewModel
      {
        Title = "连接失败",
        Message = ex.Message,
        IsLightDismissEnabled = false
      });
      return;
    }
    
    ClearFolder(App.Current.DownloadMapPath);

    var messageVm = new MessageDialogViewModel
    {
      Title = "提示",
      Message = "地图下载中",
      IsLightDismissEnabled = false,
      IsShowConfirmButton = false
    };
    var result = await _dialogService.ShowDialogAsync(messageVm, DownloadMapAsync());
    if (result.ResponseStatus != RequestResult.Success)
    {
      messageVm.Title = "错误";
      messageVm.Message = "地图下载失败";
      messageVm.IsLightDismissEnabled = true;
      return;
    }

    var mapDirPath = Path.Combine(App.Current.DownloadMapPath, Path.GetFileNameWithoutExtension(result.FileName));
    await ZipFile.ExtractToDirectoryAsync(Path.Combine(App.Current.DownloadMapPath, result.FileName), mapDirPath);
    var map = _mapFileService.LoadMap(mapDirPath);
    // var mapHash = await File.ReadAllTextAsync(Path.Combine(mapDirPath,MapFileService.HashFileName));

    var response = await JoinRoomAsync();
    if (response.ResponseStatus != RequestResult.Success)
    {
      await _dialogService.ShowDialogAsync(
        new MessageDialogViewModel
        {
          Title = "提示",
          Message = response.Message
        });
      return;
    }

    _gameManager.RoomOwnerUuid = response.RoomOwnerUuid;
    // if (map is null)
    // {
    //   await _dialogService.ShowDialogAsync(
    //     new MessageDialogViewModel
    //     {
    //       Title = "提示",
    //       Message = "地图文件损坏"
    //     });
    //   return;
    // }

    App.Current.Services.GetRequiredService<NavigationService>().Navigation(
      new RoomPageViewModel(map, _gameManager));
    _gameManager.SelectedMap = map;
  }

  private async Task<JoinRoomResponse> JoinRoomAsync(CancellationToken cancellation = default)
  {
    var joinRoomRequest = new JoinRoomRequest { Player = _gameManager.LocalPlayer };
    return (JoinRoomResponse)await _gameManager.NetClientInstance.SendRequestAsync(joinRoomRequest, cancellation);
  }

  private async Task<DownloadMapResponse> DownloadMapAsync(CancellationToken cancellation = default)
  {
    var downloadMapRequest = new DownloadMapRequest();
    return (DownloadMapResponse)await _gameManager.NetClientInstance.SendRequestAsync(downloadMapRequest, cancellation);
  }

  public void Dispose()
  {
    if (_disposed) return;
    _disposed = true;
    NoMapImage.Dispose();
    _timer.Stop();
  }

  void ClearFolder(string folderPath)
  {
    // 如果文件夹不存在，直接跳过
    if (!Directory.Exists(folderPath))
      return;

    // 删除所有文件
    foreach (var file in Directory.GetFiles(folderPath))
    {
      File.Delete(file);
    }

    // 删除所有子文件夹
    foreach (var dir in Directory.GetDirectories(folderPath))
    {
      Directory.Delete(dir, true);
    }
  }
}