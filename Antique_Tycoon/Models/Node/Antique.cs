using System;
using System.Text.Json.Serialization;
using Antique_Tycoon.Extensions;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Node;

public class Antique : ObservableObject,IDisposable
{
  public string Uuid { get; set; } = Guid.NewGuid().ToString();
  
  /// <summary>
  /// 用于保存地图后，确定是不是同一种古玩，Uuid是不同的
  /// </summary>
  public int Index { get; set; }
  
  public string Name { get; set; } = "";
  public int Value { get; set; }

  public string ImageHash { get; set; } = "";

  [JsonIgnore]
  public Bitmap Image
  {
    get;
    set => SetProperty(ref field, value);
  } = new(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Antique/Iron.png")));

  public int Dice { get; set; } = 1;
  
  private bool _disposed;

  public void Dispose()
  {
    if (_disposed) return;
    Image.Dispose();
    _disposed = true;
  }
}