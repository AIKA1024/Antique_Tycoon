using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Antique_Tycoon.Converters.JsonConverter;
using Antique_Tycoon.Models.Connections;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Node;


public abstract partial class NodeModel : CanvasItemModel, IDisposable
{
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

  // [JsonConverter(typeof(ColorJsonConverter))]
  // public Color Background
  // {
  //   get;
  //   set => SetProperty(ref field, value);
  // } = Color.Parse("#eccc68");

  public Stretch CoverStretch
  {
    get;
    set => SetProperty(ref field, value);
  }

  [JsonIgnore]
  public Bitmap Cover
  {
    get;
    set => SetProperty(ref field, value);
  } = new(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Avatar/Minecraft.png")));

  public ConnectorJsonModel[] ConnectorModels { get; set; } =
    [new(), new(), new(), new()];
  
  [JsonIgnore]
  [ObservableProperty]
  public partial Player? Owner { get; set; }

  [JsonIgnore]
  public ObservableCollection<Player> PlayersHere { get; set; } = [];

  private bool _disposed;
  public void Dispose()
  {
    if (_disposed) return;
    Cover.Dispose();
    _disposed = true;
  }
}