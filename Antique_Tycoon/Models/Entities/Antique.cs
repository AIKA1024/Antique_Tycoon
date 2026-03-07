using System;
using System.Text.Json.Serialization;
using Antique_Tycoon.Utilities;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Entities;

public partial class Antique : EntityBase
{
  /// <summary>
  /// 用于保存地图后，确定是不是同一种古玩，Uuid是不同的
  /// </summary>
  public int Index { get; set; }

  public string FlavorText { get; set; } = "这是古玩描述文本";

  public decimal Value { get; set; }
  public string ImageHash { get; set; } = "";
  
  [JsonIgnore]
  [ObservableProperty]
  public partial Bitmap Image { get; set; } =
    new(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Antique/Iron.png")));//todo 感觉不应该有一个Image，而是使用ImageHash和值转换器，Image会在网络发送中丢失

  public int Dice { get; set; } = 1;
}