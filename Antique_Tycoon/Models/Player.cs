using System;
using System.Collections.ObjectModel;
using System.Net.Sockets;
using System.Text.Json.Serialization;
using Antique_Tycoon.Models.Node;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models;

public partial class Player : ObservableObject,IDisposable
{
  public string Uuid = Guid.NewGuid().ToString();
  [ObservableProperty] string _name = "史蒂夫";
  [ObservableProperty] int _money;
  [ObservableProperty] private Bitmap _avatar = Bitmap.DecodeToHeight(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Avatar/Steve.png")),64);
  [JsonIgnore] public long LastHeartbeat { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
  public ObservableCollection<Antique> Antiques { get; set; } = [];
  public bool IsHomeowner;
  public void Dispose()
  {
    Avatar.Dispose();
  }
}