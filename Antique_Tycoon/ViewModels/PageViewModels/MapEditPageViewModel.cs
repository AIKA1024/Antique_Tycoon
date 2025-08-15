using System;
using System.Threading.Tasks;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Node;
using Antique_Tycoon.Services;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class MapEditPageViewModel : PageViewModelBase
{
  public AvaloniaList<CanvasItemModel> SelectedMapEntities { get; } = [];

  [ObservableProperty] public partial Map Map { get; set; }

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(SelectedMapEntity))]
  public partial CanvasItemModel TempSelectedMapEntity { get; set; }

  public NodeModel? SelectedMapEntity
  {
    get
    {
      if (SelectedMapEntities.Count > 1) return null;
      return TempSelectedMapEntity as NodeModel;
    }
  }

  public Point PointerPosition { get; set; }

  public Func<Bitmap>? RequestRenderControl { get; set; }

  public MapEditPageViewModel(Map map)
  {
    Map = map;
    SelectedMapEntities.CollectionChanged += (_, __) =>
    {
      OnPropertyChanged(nameof(SelectedMapEntity));
    };
  }

  [RelayCommand]
  private void CreateEntity(string type)
  {
    Dispatcher.UIThread.Post(() =>
    {
      switch (type)
      {
        case "玩家出生点":
          Map.Entities.Add(new SpawnPoint
          {
            Left = PointerPosition.X, Top = PointerPosition.Y, Title = "玩家出生点", Background = Map.NodeDefaultBackground
          });
          break;
        case "地产":
          Map.Entities.Add(new Estate
          {
            Left = PointerPosition.X, Top = PointerPosition.Y, Title = "某生态群系", Background = Map.NodeDefaultBackground
          });
          break;
        case "自定义事件":
          break;
      }
    }, DispatcherPriority.Render);
  }

  [RelayCommand]
  private void RemoveEntity(NodeModel target)
  {
    foreach (var model in target.ConnectorModels)
      model.CancelConnects(Map);
    Map.Entities.Remove(target);
  }

  [RelayCommand]
  private async Task ChangeImage(NodeModel target)
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
      target.Cover.Dispose();
      target.Cover = new Bitmap(stream);
    }
  }

  [RelayCommand]
  private async Task SaveMap()
  {
    Map.Cover = RequestRenderControl?.Invoke();
    await App.Current.Services.GetRequiredService<MapFileService>().SaveMapAsync(Map);
  }
}