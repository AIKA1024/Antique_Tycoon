using Antique_Tycoon.Converters.JsonConverter;
using Avalonia;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Text.Json.Serialization;
using Antique_Tycoon.Models.Entities;
using Antique_Tycoon.Models.Entities.StaffImpls;
using CanvasItemModel = Antique_Tycoon.Models.Nodes.CanvasItemModel;
using Color = Avalonia.Media.Color;
using NodeModel = Antique_Tycoon.Models.Nodes.NodeModel;

namespace Antique_Tycoon.Models;

public partial class Map : ObservableObject
{
  [ObservableProperty] public partial string Name { get; set; } = string.Empty;
  public int MaxPlayer { get; private set; } = 5;

  [ObservableProperty]
  [JsonConverter(typeof(PointJsonConverter))]
  public partial Point Offset { get; set; }

  [ObservableProperty] [JsonIgnore] public partial Bitmap Cover { get; set; }

  [ObservableProperty] public partial double Scale { get; set; } = 1;

  [ObservableProperty] public partial double CanvasHeight { get; set; } = 2000;

  [ObservableProperty] public partial double CanvasWidth { get; set; } = 3600;

  [ObservableProperty] public partial int StartingCash { get; set; } = 10000;

  [ObservableProperty] public partial int SpawnPointCashReward { get; set; } = 2000;

  [ObservableProperty]
  [JsonIgnore] // 这个值通过另外的Hash文件读取
  public partial string Hash { get; set; } = "";

  [ObservableProperty]
  [JsonConverter(typeof(ColorJsonConverter))]
  public partial Color CanvasBackground { get; set; } = Color.Parse("#262626");

  [JsonIgnore] public NodeModel SpawnNode => (NodeModel)EntitiesDict[SpawnNodeUuid];

  public string SpawnNodeUuid { get; set; } = "";

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

  [JsonIgnore] public Dictionary<string, CanvasItemModel> EntitiesDict { get; } = [];

  public ObservableCollection<Antique> Antiques { get; set; } = [];

  public ObservableCollection<IStaff> Staffs { get; set; } =
  [
    new CardSharp(),
    new WelfareCheat(),
    new MineOwner(),
    new TaxLord()
  ];

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