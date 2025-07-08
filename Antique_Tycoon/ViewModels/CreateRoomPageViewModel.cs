using System;
using Antique_Tycoon.Services;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels;

public partial class CreateRoomPageViewModel:ViewModelBase,IDisposable
{
  [ObservableProperty] private string _roomName;

  [ObservableProperty] private Bitmap _cover;

  public CreateRoomPageViewModel()
  {
    Cover = Bitmap.DecodeToWidth(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Map.jpg")),512,BitmapInterpolationMode.LowQuality );
  }
  public void Dispose()
  {
    Cover.Dispose();
  }

  [RelayCommand]
  private void CreateRoomAndNavigateToRoomPage()
  {
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new RoomPageViewModel());
  }
}