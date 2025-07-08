using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

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
}