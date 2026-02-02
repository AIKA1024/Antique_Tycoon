using System;
using System.Text.Json.Serialization;
using Antique_Tycoon.Utilities;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Entities;

public class Antique : EntityBase
{
  /// <summary>
  /// 用于保存地图后，确定是不是同一种古玩，Uuid是不同的
  /// </summary>
  public int Index { get; set; }

  public string FlavorText { get; set; } = "这是古玩描述文本";
  
  public decimal Value { get; set; }
  public string ImageHash { get; set; } = "";

  [JsonIgnore]
  public Bitmap Image
  {
    get;
    set => SetProperty(ref field, value);
  } = ImageHelper.GetBitmap("avares://Antique_Tycoon/Assets/Image/Antique/Iron.png");

  public int Dice { get; set; } = 1;
}