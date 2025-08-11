using System;
using System.Text.Json.Serialization;
using Antique_Tycoon.Models.Connections;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Node;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]

[JsonDerivedType(typeof(Connection), "Connection")]
[JsonDerivedType(typeof(SpawnPoint), "SpawnPoint")]
[JsonDerivedType(typeof(Estate), "Estate")]
//[JsonDerivedType(typeof(Antique), "Antique")]
public partial class CanvasItemModel:ObservableObject
{
  [ObservableProperty] public partial string Uuid { get; set; } = Guid.NewGuid().ToString();
}