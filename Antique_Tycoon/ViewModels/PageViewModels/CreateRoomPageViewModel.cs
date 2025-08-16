using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Models;
using Antique_Tycoon.Net;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class CreateRoomPageViewModel : PageViewModelBase, IDisposable
{
  private bool _disposed;
  [ObservableProperty] private string _roomName = $"{App.Current.Services.GetRequiredService<Player>().Name}的房间";

  [ObservableProperty] private Bitmap _cover;
  public Map SelectedMap { get; set; }

  private CancellationTokenSource _cts = new();

  public CreateRoomPageViewModel()
  {
    Cover = Bitmap.DecodeToWidth(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Map.jpg")), 512,
      BitmapInterpolationMode.LowQuality);
  }

  public void Dispose()
  {
    if (_disposed) return;
    Cover.Dispose();
    _disposed = true;
  }

  [RelayCommand]
  private async Task CreateRoomAndNavigateToRoomPage()
  {
    _cts.TryReset();
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new RoomPageViewModel(_cts));
    try
    {
      await App.Current.Services.GetRequiredService<NetServer>()
        .CreateRoomAndListenAsync(RoomName, SelectedMap, _cts.Token);
    }
    catch (OperationCanceledException e)
    {
      // Console.WriteLine(e);
    }
  }

  [RelayCommand]
  private async Task ShowSelectMapDialog()
  {
    var services = App.Current.Services;
    await services.GetRequiredService<DialogService>()
      .ShowDialogAsync(new SelectMapDialogViewModel(services.GetRequiredService<MapFileService>().GetMaps()));
  }
}