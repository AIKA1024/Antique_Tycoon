using System;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Models;
using Antique_Tycoon.Net;
using Antique_Tycoon.Services;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class CreateRoomPageViewModel:PageViewModelBase,IDisposable
{
  
  [ObservableProperty] private string _roomName = $"{App.Current.Services.GetRequiredService<Player>().Name}的房间";

  [ObservableProperty] private Bitmap _cover;

  private CancellationTokenSource cts = new();
  public CreateRoomPageViewModel()
  {
    Cover = Bitmap.DecodeToWidth(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Map.jpg")),512,BitmapInterpolationMode.LowQuality );
  }
  public void Dispose()
  {
    Cover.Dispose();
  }

  [RelayCommand]
  private async Task CreateRoomAndNavigateToRoomPage()
  {
    cts.TryReset();
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new RoomPageViewModel(cts));
    try
    {
      await App.Current.Services.GetRequiredService<NetServer>().CreateRoomAndListenAsync(RoomName,cts.Token);
    }
    catch (OperationCanceledException e)
    {
      // Console.WriteLine(e);
    }
  }
}