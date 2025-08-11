using System;
using System.Text.Json.Serialization;
using Antique_Tycoon.Converters.JsonConverter;
using Antique_Tycoon.Models.Node;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Color = Avalonia.Media.Color;

namespace Antique_Tycoon.Models;

public class Map : ObservableObject
{
  public string Name
  {
    get;
    set => SetProperty(ref field, value);
  } = string.Empty;

  public double Scale
  {
    get;
    set => SetProperty(ref field, value);
  } = 1;
  [JsonConverter(typeof(PointJsonConverter))]
  public Point Offset
  {
    get;
    set => SetProperty(ref field, value);
  }

  [JsonIgnore]
  public Bitmap Cover
  {
    get;
    set => SetProperty(ref field, value);
  }

  public double CanvasHeight
  {
    get;
    set => SetProperty(ref field, value);
  } = 2000;

  public double CanvasWidth
  {
    get;
    set => SetProperty(ref field, value);
  } = 3600;

  [JsonConverter(typeof(ColorJsonConverter))]
  public Color CanvasBackground
  {
    get;
    set => SetProperty(ref field, value);
  } = Color.Parse("#262626");

  [JsonConverter(typeof(ColorJsonConverter))]
  public Color NodeDefaultBackground
  {
    get;
    set => SetProperty(ref field, value);
  } = Color.Parse("#eccc68");

  public AvaloniaList<CanvasItemModel> Entities { get; set; } = [];
}