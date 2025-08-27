using Antique_Tycoon.Converters.JsonConverter;
using Antique_Tycoon.Models.Node;
using Avalonia;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json.Serialization;
using Color = Avalonia.Media.Color;

namespace Antique_Tycoon.Models;

public partial class Map : ObservableObject
{
  [ObservableProperty] public partial string Name { get; set; } = string.Empty;

  [ObservableProperty]
  [JsonConverter(typeof(PointJsonConverter))]
  public partial Point Offset { get; set; }

  [ObservableProperty]
  [JsonIgnore] public partial Bitmap Cover { get; set; }

  [ObservableProperty]
  public partial double Scale { get; set; } = 1;

  [ObservableProperty] public partial double CanvasHeight { get; set; } = 2000;

  [ObservableProperty] public partial double CanvasWidth { get; set; } = 3600;

  [ObservableProperty]
  [JsonConverter(typeof(ColorJsonConverter))]
  public partial Color CanvasBackground { get; set; } = Color.Parse("#262626");

  [ObservableProperty]
  [JsonConverter(typeof(ColorJsonConverter))]
  public partial Color NodeDefaultBackground { get; set; } = Color.Parse("#eccc68");

  public ObservableCollection<CanvasItemModel> Entities
  {
    get;
    set
    {
      field.CollectionChanged -= EntitiesOnCollectionChanged;
      field = value;
      field.CollectionChanged += EntitiesOnCollectionChanged;
      EntitiesDict.Clear();
      foreach (var entity in field)
        EntitiesDict[entity.Uuid] = entity;
    }
  } = [];

  [JsonIgnore] public Dictionary<string, CanvasItemModel> EntitiesDict = [];

  public Map()
  {
    Entities.CollectionChanged += EntitiesOnCollectionChanged;
  }


  private void EntitiesOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    switch (e.Action)
    {
      case NotifyCollectionChangedAction.Add:
        foreach (CanvasItemModel item in e.NewItems)
          EntitiesDict.Add(item.Uuid, item);
        break;
      case NotifyCollectionChangedAction.Remove:
        foreach (CanvasItemModel item in e.OldItems)
          EntitiesDict.Remove(item.Uuid);
        break;
      case NotifyCollectionChangedAction.Replace:
        foreach (CanvasItemModel item in e.OldItems)
          EntitiesDict.Remove(item.Uuid);
        foreach (CanvasItemModel item in e.NewItems)
          EntitiesDict.Add(item.Uuid, item);
        break;
      case NotifyCollectionChangedAction.Reset:
        EntitiesDict.Clear();
        break;
    }
  }
}