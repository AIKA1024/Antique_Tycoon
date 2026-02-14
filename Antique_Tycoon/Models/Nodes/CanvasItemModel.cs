using System;
using System.Text.Json.Serialization;
using Antique_Tycoon.Models.Connections;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Nodes;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]

[JsonDerivedType(typeof(Connection), "Connection")]
[JsonDerivedType(typeof(SpawnPoint), "SpawnPoint")]
[JsonDerivedType(typeof(Estate), "Estate")]
[JsonDerivedType(typeof(Mine), "Mine")]
[JsonDerivedType(typeof(TalentMarket), "TalentMarket")]
public partial class CanvasItemModel:ObservableObject
{
  [ObservableProperty] public partial string Uuid { get; set; } = Guid.NewGuid().ToString();
  [ObservableProperty] public partial int ZIndex { get; set; }
}