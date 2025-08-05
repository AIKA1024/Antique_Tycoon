using System;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
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
using CanvasEntity = Antique_Tycoon.Models.Node.CanvasEntity;
using SpawnPoint = Antique_Tycoon.Models.Node.SpawnPoint;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class MapEditPageViewModel : DragAndZoomViewModel
{
  public AvaloniaList<CanvasEntity> SelectedMapEntities { get; } = [];

  [ObservableProperty] private Map _map;

  [ObservableProperty] private CanvasEntity? _selectedMapEntity;

  public Point PointerPosition { get; set; }

  public Func<Bitmap>? RequestRenderControl { get; set; }

  public MapEditPageViewModel(Map map)
  {
    Map = map;
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
  private void RemoveEntity(CanvasEntity target)
  {
    Map.Entities.Remove(target);
  }

  [RelayCommand]
  private async Task ChangeImage(CanvasEntity target)
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
  
  // [RelayCommand]
  // private void ZoomToFit()
  // {
  //   if (Map.Entities.Count == 0)
  //     return;
  //
  //   var bounds = GetCanvasContentBounds(canvas);
  //
  //   double scaleX = targetSize.Width / bounds.Width;
  //   double scaleY = targetSize.Height / bounds.Height;
  //
  //   double scale = Math.Min(scaleX, scaleY); // 保持纵横比
  //
  //   double offsetX = -bounds.X * scale + (targetSize.Width - bounds.Width * scale) / 2;
  //   double offsetY = -bounds.Y * scale + (targetSize.Height - bounds.Height * scale) / 2;
  //
  //   Offset = new Point(offsetX, offsetY);
  //   Scale = scale;
  // }
  //
  // private static Rect GetCanvasContentBounds()
  // {
  //   double left = double.PositiveInfinity;
  //   double top = double.PositiveInfinity;
  //   double right = double.NegativeInfinity;
  //   double bottom = double.NegativeInfinity;
  //
  //   foreach (var entity in Map.Entities)
  //   {
  //     double x = entity.Left;
  //     double y = entity.Top;
  //
  //     if (double.IsNaN(x)) x = 0;
  //     if (double.IsNaN(y)) y = 0;
  //
  //     var bounds = entity.Bounds; // Avalonia.Visual.Bounds
  //     double width = bounds.Width;
  //     double height = bounds.Height;
  //
  //     left = Math.Min(left, x);
  //     top = Math.Min(top, y);
  //     right = Math.Max(right, x + width);
  //     bottom = Math.Max(bottom, y + height);
  //   }
  //
  //   // 如果没有元素，返回 Rect(0,0,0,0)
  //   if (double.IsPositiveInfinity(left) || double.IsPositiveInfinity(top))
  //     return new Rect(0, 0, 0, 0);
  //
  //   return new Rect(left, top, right - left, bottom - top);
  // }
}