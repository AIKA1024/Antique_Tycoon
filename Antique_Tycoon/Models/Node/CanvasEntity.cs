using System;
using System.Text.Json.Serialization;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Node;

public abstract partial class CanvasEntity : ObservableObject,IDisposable
{
  public string Uuid
  {
    get;
    set => SetProperty(ref field, value);
  } = Guid.CreateVersion7().ToString();

  public double Left
  {
    get;
    set => SetProperty(ref field, value);
  }

  public double Top
  {
    get;
    set => SetProperty(ref field, value);
  }

  public string Title
  {
    get;
    set => SetProperty(ref field, value);
  } = "";

  public double Width
  {
    get;
    set => SetProperty(ref field, value);
  } = 120;

  public double Height
  {
    get;
    set => SetProperty(ref field, value);
  } = 150;

  public Color Background
  {
    get;
    set => SetProperty(ref field, value);
  } = Color.Parse("#eccc68");

  public Stretch CoverStretch
  {
    get;
    set => SetProperty(ref field, value);
  }

  public Bitmap Cover
  {
    get;
    set => SetProperty(ref field, value);
  } = new(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Avatar/Minecraft.png")));

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