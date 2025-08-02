using System;
using System.Text.Json.Serialization;
using Antique_Tycoon.Models.Node;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models;

public partial class Map : ObservableObject
{
  public string Name
  {
    get;
    set => SetProperty(ref field, value);
  } = string.Empty;

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

  public IBrush CanvasBackground
  {
    get;
    set => SetProperty(ref field, value);
  } = new SolidColorBrush(Color.Parse("#262626"));

  public IBrush NodeDefaultBackground
  {
    get;
    set => SetProperty(ref field, value);
  } = new SolidColorBrush(Color.Parse("#eccc68"));

  public AvaloniaList<CanvasEntity> Entities { get; set; } = [];
}