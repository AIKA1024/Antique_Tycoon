using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.Json.Serialization;
using Antique_Tycoon.Converters.JsonConverter;
using Antique_Tycoon.Models.Node;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using Color = Avalonia.Media.Color;

namespace Antique_Tycoon.Models;

public partial class Map : ObservableObject
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

  public AvaloniaList<CanvasItemModel> Entities
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