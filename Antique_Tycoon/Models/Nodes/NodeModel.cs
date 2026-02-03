using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Antique_Tycoon.Models.Connections;
using Antique_Tycoon.Services;
using Antique_Tycoon.Utilities;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Models.Nodes;

public abstract partial class NodeModel : CanvasItemModel
{
  const double DefaultWidth = 120;
  const double DefaultHeight = 150;

  public double Left
  {
    get;
    set => SetProperty(ref field, value);
  }

  public double Top
  {
    get;
    set => SetProperty(ref field, value);
  }

  public string Title
  {
    get;
    set => SetProperty(ref field, value);
  } = "";

  public double? Width
  {
    get;
    set => SetProperty(ref field, value);
  } = DefaultWidth;

  public double? Height
  {
    get;
    set => SetProperty(ref field, value);
  } = DefaultHeight;

  public bool IsAutoSize
  {
    get;
    set
    {
      if (value)
      {
        Width = null;
        Height = null;
      }
      else
      {
        double.TryParse(WidthDisplayText, out var width);
        double.TryParse(HeightDisplayText, out var height);
        if (width != 0 && height != 0)
        {
          Width = width;
          Height = height;
        }
      }

      SetProperty(ref field, value);
      OnPropertyChanged(nameof(WidthDisplayText));
    }
  }

  [JsonIgnore]
  public string WidthDisplayText
  {
    get;
    set
    {
      if (!IsAutoSize)
      {
        // 当用户在 TextBox 输入并回车/失去焦点时
        if (double.TryParse(value, out var result))
        {
          Width = result;
        }
        else
        {
          Width = double.NaN; // 解析失败（如输入空）则回退到 Auto
        }
      }

      SetProperty(ref field, value);
    }
  }

  [JsonIgnore]
  public string HeightDisplayText
  {
    get;
    set
    {
      if (!IsAutoSize)
      {
        // 当用户在 TextBox 输入并回车/失去焦点时
        if (double.TryParse(value, out var result))
        {
          Height = result;
        }
        else
        {
          Height = double.NaN; // 解析失败（如输入空）则回退到 Auto
        }
      }

      SetProperty(ref field, value);
    }
  }

  // [JsonConverter(typeof(ColorJsonConverter))]
  // public Color Background
  // {
  //   get;
  //   set => SetProperty(ref field, value);
  // } = Color.Parse("#eccc68");

  public Stretch CoverStretch
  {
    get;
    set => SetProperty(ref field, value);
  }

  public string ImageHash { get; set; } = "";

  [ObservableProperty]
  [JsonIgnore]
  public partial Bitmap Image { get; set; } =
    new(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Avatar/Minecraft.png")));

  public ConnectorJsonModel[] ConnectorModels { get; set; } =
    [new(), new(), new(), new()];

  [JsonIgnore] [ObservableProperty] public partial Player? Owner { get; set; }

  [JsonIgnore] public ObservableCollection<Player> PlayersHere { get; set; } = [];
}