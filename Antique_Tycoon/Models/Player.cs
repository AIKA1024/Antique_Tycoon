using System;
using System.Collections.ObjectModel;
using System.Text.Json.Serialization;
using Antique_Tycoon.Models.Node;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models;

public partial class Player : ObservableObject, IDisposable
{
  public string Uuid { get; set; } = Guid.NewGuid().ToString();

  [ObservableProperty] public partial string Name { get; set; } = "史蒂夫";

  [ObservableProperty] public partial int Money { get; set; }

  public string CurrentNodeUuId { get; set; } = "";

  [JsonIgnore] 
  [ObservableProperty]
  public partial Bitmap Avatar { get; private set; } = Bitmap.DecodeToHeight(
    AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Avatar/Steve.png")), 64);

  public ObservableCollection<Antique> Antiques { get; set; } = [];
  
  public ObservableCollection<Estate>  Estates { get; set; } = [];
  

  public PlayerRole Role // todo 违反了开闭原则，但小项目不管了
  {
    // ReSharper disable once PropertyFieldKeywordIsNeverAssigned
    get;
    set
    {
      Avatar.Dispose();
      Avatar = value switch
      {
        PlayerRole.Steve => Bitmap.DecodeToHeight(
          AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Avatar/Steve.png")), 64),
        PlayerRole.Pig => Bitmap.DecodeToHeight(
          AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Avatar/Pig.png")), 64),
        PlayerRole.Cow => Bitmap.DecodeToHeight(
          AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Avatar/Cow.png")), 64),
        PlayerRole.Creeper => Bitmap.DecodeToHeight(
          AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Avatar/Creeper.png")), 64),
        PlayerRole.Sheep => Bitmap.DecodeToHeight(
          AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Avatar/Sheep.png")), 64),
        PlayerRole.Villager => Bitmap.DecodeToHeight(
          AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Avatar/Villager.png")), 64),
        PlayerRole.Zombie => Bitmap.DecodeToHeight(
          AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/Image/Avatar/Zombie.png")), 64),
        _ => Avatar
      };
      field = value;
    }
  } = PlayerRole.Steve;

  public void Dispose()
  {
    Avatar.Dispose();
    GC.SuppressFinalize(this);
  }
}

public enum PlayerRole
{
  Steve,
  Pig,
  Cow,
  Creeper,
  Sheep,
  Villager,
  Zombie
}