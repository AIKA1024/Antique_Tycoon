using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Node;

public partial class CanvasItemModel:ObservableObject
{
  [ObservableProperty] public partial string Uuid { get; set; } = Guid.NewGuid().ToString();
}