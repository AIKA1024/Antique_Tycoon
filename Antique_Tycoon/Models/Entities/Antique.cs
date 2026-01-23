using System;
using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Entities;

public class Antique : EntityBase, IDisposable
{
  /// <summary>
  /// 用于保存地图后，确定是不是同一种古玩，Uuid是不同的
  /// </summary>
  public int Index { get; set; }

  public string FlavorText { get; set; } = "这是古玩描述文本";
  
  public int Value { get; set; }
  public string ImageHash { get; set; } = "";

  [JsonIgnore]
  public Bitmap Image
  {
    get;
    set => SetProperty(ref field, value);
  } = new(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Antique/Iron.png")));

  public int Dice { get; set; } = 1;

  private bool _isDisposed;

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

  protected virtual void Dispose(bool disposing)
  {
    if (_isDisposed) return;
    
    if (disposing)
      Image.Dispose();
    _isDisposed = true;
  }
}