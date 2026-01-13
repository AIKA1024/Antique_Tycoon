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
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Antique_Tycoon.Models.Entities;
using Antique_Tycoon.Models.Nodes;
using CanvasItemModel = Antique_Tycoon.Models.Nodes.CanvasItemModel;
using NodeModel = Antique_Tycoon.Models.Nodes.NodeModel;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class MapEditPageViewModel : PageViewModelBase
{
  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(SelectedMapEntity))]
  public partial ObservableCollection<CanvasItemModel> SelectedMapEntities { get; set; } = [];

  [ObservableProperty] public partial Map CurrentMap { get; set; }

  [ObservableProperty]
  [NotifyPropertyChangedFor(nameof(SelectedMapEntity))]
  public partial CanvasItemModel TempSelectedMapEntity { get; set; } //用于筛选是不是线条

  public ObservableCollection<AntiqueMapItem> AntiqueMapItems { get; } = [];

  public NodeModel? SelectedMapEntity
  {
    get
    {
      if (SelectedMapEntities.Count > 1) return null; //多选了
      return TempSelectedMapEntity as NodeModel;
    }
  }

  public Func<Bitmap>? RequestRenderControl { get; set; }

  public MapEditPageViewModel(Map currentMap)
  {
    CurrentMap = currentMap;
    foreach (var antique in CurrentMap.Antiques)
    {
      if (antique.Index >= AntiqueMapItems.Count || AntiqueMapItems[antique.Index] == null)//由于下面使用的Insert，所以有可能是null
        AntiqueMapItems.Insert(antique.Index, new  AntiqueMapItem(antique, 1));
      else
        AntiqueMapItems[antique.Index].Amount += 1;
    }
  }

  partial void OnSelectedMapEntitiesChanged(ObservableCollection<CanvasItemModel> oldValue,
    ObservableCollection<CanvasItemModel> newValue) =>
    newValue.CollectionChanged += (_, __) => OnPropertyChanged(nameof(SelectedMapEntity));

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
      var  item = AntiqueMapItems[i];
      for (int j = 0; j < item.Amount; j++)
      {
        CurrentMap.Antiques.Add(new Antique
        {
          Index = i,
          Dice = item.AntiqueItem.Dice,
          Name = item.AntiqueItem.Name,
          Image =  item.AntiqueItem.Image,
          Value =  item.AntiqueItem.Value
        });
      }
    }

      
    await App.Current.Services.GetRequiredService<MapFileService>().SaveMapAsync(CurrentMap);
  }
}