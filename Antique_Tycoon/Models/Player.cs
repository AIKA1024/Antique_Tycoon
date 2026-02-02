using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using Antique_Tycoon.Models.Effects;
using Antique_Tycoon.Models.Entities;
using Antique_Tycoon.Models.Enums;
using Antique_Tycoon.Models.Nodes;
using Antique_Tycoon.Utilities;
using Avalonia;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models;

public partial class Player : ObservableObject
{
  public string Uuid { get; set; } = Guid.NewGuid().ToString();

  [ObservableProperty] public partial string Name { get; set; } = "史蒂夫";

  [ObservableProperty] public partial decimal Money { get; set; }

  public string CurrentNodeUuId { get; set; } = "";

  [JsonIgnore] 
  [ObservableProperty]
  public partial Bitmap Avatar { get; private set; } = ImageHelper.GetBitmap("avares://Antique_Tycoon/Assets/Image/Avatar/Steve.png");

  public ObservableCollection<Antique> Antiques { get; set; } = [];
  
  public ObservableCollection<Estate> Estates { get; set; } = [];
  
  public ObservableCollection<IStaff> StaffList { get; set; } = [];
  

  public PlayerRole Role // todo 违反了开闭原则，但小项目不管了
  {
    // ReSharper disable once PropertyFieldKeywordIsNeverAssigned
    get;
    set
    {
      Avatar = value switch
      {
        PlayerRole.Steve => ImageHelper.GetBitmap("avares://Antique_Tycoon/Assets/Image/Avatar/Steve.png"),
        PlayerRole.Pig => ImageHelper.GetBitmap("avares://Antique_Tycoon/Assets/Image/Avatar/Pig.png"),
        PlayerRole.Cow => ImageHelper.GetBitmap("avares://Antique_Tycoon/Assets/Image/Avatar/Cow.png"),
        PlayerRole.Creeper => ImageHelper.GetBitmap("avares://Antique_Tycoon/Assets/Image/Avatar/Creeper.png"),
        PlayerRole.Sheep => ImageHelper.GetBitmap("avares://Antique_Tycoon/Assets/Image/Avatar/Sheep.png"),
        PlayerRole.Villager => ImageHelper.GetBitmap("avares://Antique_Tycoon/Assets/Image/Avatar/Villager.png"),
        PlayerRole.Zombie => ImageHelper.GetBitmap("avares://Antique_Tycoon/Assets/Image/Avatar/Zombie.png"),
        _ => Avatar
      };
      field = value;
    }
  } = PlayerRole.Steve;
  
  public IEnumerable<IStaffEffect> GetActiveEffects(GameTriggerPoint point)
  {
    return StaffList
      .SelectMany(s => s.Effects)
      .Where(e => e.TriggerPoint == point);
  }
}

