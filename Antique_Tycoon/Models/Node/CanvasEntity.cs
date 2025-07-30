using System;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Node;

public abstract partial class CanvasEntity : ObservableObject,IDisposable
{
  [ObservableProperty] private double _left;
  [ObservableProperty] private double _top;
  [ObservableProperty] private string _title;
  [ObservableProperty] private double _width = 120;
  [ObservableProperty] private double _height = 150;
  [ObservableProperty] private IBrush _background = new SolidColorBrush(Color.Parse("#eccc68"));
  [ObservableProperty] private Stretch _coverStretch;
  [ObservableProperty] private Bitmap _cover = new(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Avatar/Minecraft.png")));
  private bool _disposed;
  public void Dispose()
  {
    Dispose(disposing: true);
    GC.SuppressFinalize(this);
  }
  protected virtual void Dispose(bool disposing)
  {
    if (_disposed) return;

    if (disposing)
      Cover.Dispose();
    _disposed = true;
  }
}