using Antique_Tycoon.Extensions;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Node;
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
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class MapEditPageViewModel : PageViewModelBase
{
  public ObservableCollection<CanvasItemModel> SelectedMapEntities { get; set; } = [];

  [ObservableProperty] public partial Map CurrentMap { get; set; }

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

  public Func<Bitmap>? RequestRenderControl { get; set; }

  public MapEditPageViewModel(Map currentMap)
  {
    CurrentMap = currentMap;
    SelectedMapEntities.CollectionChanged += (_, __) =>
    {
      OnPropertyChanged(nameof(SelectedMapEntity));
    };
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
    CurrentMap.Cover = RequestRenderControl?.Invoke();
    await App.Current.Services.GetRequiredService<MapFileService>().SaveMapAsync(CurrentMap);
  }
}