using Antique_Tycoon.Extensions;
using Antique_Tycoon.Models;
using Antique_Tycoon.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Antique_Tycoon.Models.Entities;
using Antique_Tycoon.Models.Nodes;
using Antique_Tycoon.ViewModels.DetailViewModels;
using Antique_Tycoon.ViewModels.DialogViewModels;
using CanvasItemModel = Antique_Tycoon.Models.Nodes.CanvasItemModel;
using NodeModel = Antique_Tycoon.Models.Nodes.NodeModel;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class MapEditPageViewModel : PageViewModelBase
{
  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(SelectedMapEntity))]
  public partial ObservableCollection<CanvasItemModel> SelectedMapEntities { get; set; } = [];

  public Dictionary<NodeModel, NodeDetailViewModel> NodeDetailViewModels { get; } = [];
  [ObservableProperty] public partial Map CurrentMap { get; set; }

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(SelectedMapEntity))]
  [NotifyPropertyChangedFor(nameof(SelectedMapEntityViewModel))]
  public partial CanvasItemModel TempSelectedMapEntity { get; set; } //用于筛选是不是线条

  public ObservableCollection<AntiqueMapItem> AntiqueMapItems { get; } = [];
  public Point PointerPosition { get; set; }


  public NodeModel? SelectedMapEntity
  {
    get
    {
      if (SelectedMapEntities.Count > 1) return null; //多选了
      return TempSelectedMapEntity as NodeModel;
    }
  }

  public NodeDetailViewModel? SelectedMapEntityViewModel
  {
    get
    {
      if (SelectedMapEntity != null &&
          NodeDetailViewModels.TryGetValue(SelectedMapEntity, out var viewModel))
        return viewModel;
      return null;
    }
  }

  public Func<Bitmap>? RequestRenderControl { get; set; }

  public MapEditPageViewModel(Map currentMap)
  {
    CurrentMap = currentMap;
    foreach (var canvasItemModel in CurrentMap.Entities)
      if (canvasItemModel is NodeModel nodeModel)
        NodeDetailViewModels.Add(nodeModel, CreateViewModelForEntity(nodeModel));
    foreach (var antique in CurrentMap.Antiques)
    {
      if (antique.Index >= AntiqueMapItems.Count || AntiqueMapItems[antique.Index] == null) //由于下面使用的Insert，所以有可能是null
        AntiqueMapItems.Insert(antique.Index, new AntiqueMapItem(antique, 1));
      else
        AntiqueMapItems[antique.Index].Amount += 1;
    }
  }

  private NodeDetailViewModel CreateViewModelForEntity(NodeModel entity)
  {
    return entity switch
    {
      // 如果是地产，创建功能强大的 EstateDetailViewModel
      Estate estate => new EstateDetailViewModel(estate),
      SpawnPoint spawnPoint => new SpawnPointDetailViewModel(spawnPoint),
      Mine mine => new MineDetailViewModel(mine),

      // 如果是其他节点，创建普通的 VM
      _ => new NodeDetailViewModel(entity)
    };
  }

  partial void OnSelectedMapEntitiesChanged(ObservableCollection<CanvasItemModel> oldValue,
    ObservableCollection<CanvasItemModel> newValue) =>
    newValue.CollectionChanged += (_, _) => OnPropertyChanged(nameof(SelectedMapEntity));

  #region 添加、删除元素

  [RelayCommand]
  private void CreateEntity(string type)
  {
    Dispatcher.UIThread.Post(async void () =>
    {
      NodeModel? model = null;
      switch (type)
      {
        case "玩家出生点":
          if (string.IsNullOrEmpty(CurrentMap.SpawnNodeUuid))
          {
            var spawnPoint = new SpawnPoint
            {
              Left = PointerPosition.X,
              Top = PointerPosition.Y,
              Title = "玩家出生点",
            };
            CurrentMap.Entities.Add(spawnPoint);
            model = spawnPoint;
            CurrentMap.SpawnNodeUuid = spawnPoint.Uuid;
          }
          else
            await App.Current.Services.GetRequiredService<DialogService>().ShowDialogAsync(
              new MessageDialogViewModel
              {
                Title = "提示",
                Message = "只能有一个出生点"
              });

          break;
        case "地产":
          var estate = new Estate
          {
            Left = PointerPosition.X,
            Top = PointerPosition.Y,
            Title = "某生态群系",
          };
          CurrentMap.Entities.Add(estate);
          model = estate;
          break;
        case "矿洞":
          var mine = new Mine
          {
            Left = PointerPosition.X,
            Top = PointerPosition.Y,
            Title = "矿洞"
          };
          CurrentMap.Entities.Add(mine);
          model = mine;
          break;
        case "传送点":
          break;
      }

      if (model != null)
        NodeDetailViewModels.Add(model, CreateViewModelForEntity(model));
    }, DispatcherPriority.Render);
  }

  [RelayCommand]
  private void RemoveEntity()
  {
    for (int i = SelectedMapEntities.Count - 1; i >= 0; i--)
    {
      var target = (NodeModel)SelectedMapEntities[i];
      foreach (var model in target.ConnectorModels)
        model.CancelConnects(CurrentMap);
      if (target.Uuid == CurrentMap.SpawnNodeUuid)
        CurrentMap.SpawnNodeUuid = "";
      CurrentMap.Entities.Remove(target);
      NodeDetailViewModels.Remove(target);
    }
  }

  #endregion

  #region 切换图片

  [RelayCommand]
  private async Task ChangeNodeImage(NodeModel target)
  {
    var files = await App.Current.Services.GetRequiredService<TopLevel>().StorageProvider.OpenFilePickerAsync(
      new FilePickerOpenOptions
      {
        Title = "选择一张图片",
        AllowMultiple = false,
        FileTypeFilter =
        [
          new FilePickerFileType("Image Files")
          {
            Patterns = ["*.png", "*.jpg", "*.jpeg", "*.webp", "*.jfif", "*.bmp"]
          },
          new FilePickerFileType("All Files")
          {
            Patterns = ["*.*"]
          }
        ]
      });
    if (files.Count >= 1)
    {
      await using var stream = await files[0].OpenReadAsync();
      target.Image.Dispose();
      target.Image = new Bitmap(stream);
    }
  }

  [RelayCommand]
  private async Task ChangeAntiqueImage(AntiqueMapItem target)
  {
    var files = await App.Current.Services.GetRequiredService<TopLevel>().StorageProvider.OpenFilePickerAsync(
      new FilePickerOpenOptions
      {
        Title = "选择一张图片",
        AllowMultiple = false,
        FileTypeFilter =
        [
          new FilePickerFileType("Image Files")
          {
            Patterns = ["*.png", "*.jpg", "*.jpeg", "*.webp", "*.jfif", "*.bmp"]
          },
          new FilePickerFileType("All Files")
          {
            Patterns = ["*.*"]
          }
        ]
      });
    if (files.Count >= 1)
    {
      await using var stream = await files[0].OpenReadAsync();
      target.AntiqueItem.Image.Dispose();
      target.AntiqueItem.Image = new Bitmap(stream);
    }
  }

  #endregion

  [RelayCommand]
  private void AddNewAntique()
  {
    AntiqueMapItems.Add(new AntiqueMapItem(new Antique(), 1));
  }

  [RelayCommand]
  private void RemoveAntique(AntiqueMapItem antiqueMapItem)
  {
    AntiqueMapItems.Remove(antiqueMapItem);
  }

  [RelayCommand]
  private async Task SaveMap()
  {
    CurrentMap.Cover = RequestRenderControl?.Invoke()!;
    CurrentMap.Antiques.Clear();

    for (int i = 0; i < AntiqueMapItems.Count; i++)
    {
      var item = AntiqueMapItems[i];
      for (int j = 0; j < item.Amount; j++)
      {
        CurrentMap.Antiques.Add(new Antique
        {
          Index = i,
          Dice = item.AntiqueItem.Dice,
          Name = item.AntiqueItem.Name,
          Image = item.AntiqueItem.Image,
          Value = item.AntiqueItem.Value
        });
      }
    }


    await App.Current.Services.GetRequiredService<MapFileService>().SaveMapAsync(CurrentMap);
  }
}