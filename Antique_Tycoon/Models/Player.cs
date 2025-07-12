using System;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models;

public partial class Player : ObservableObject,IDisposable
{
  public string Uuid = Guid.CreateVersion7().ToString();
  [ObservableProperty] string _name = "史蒂夫";
  [ObservableProperty] int _money;
  [ObservableProperty] private Bitmap _avatar = Bitmap.DecodeToHeight(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Avatars/Steve.png")),64);
  [JsonIgnore] public NetworkStream Stream { get; set; }
  public AvaloniaList<Antique> Antiques { get; set; }
  public void Dispose()
  {
    Avatar.Dispose();
  }
}