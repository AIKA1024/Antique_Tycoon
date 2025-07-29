using Antique_Tycoon.Models.Node;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models;

public partial class Map : ObservableObject
{
  [ObservableProperty] private string _name = string.Empty;
  [ObservableProperty] private Bitmap _cover;
  [ObservableProperty] private double _canvasHeight;
  [ObservableProperty] private double _canvasWidth;
  public AvaloniaList<CanvasEntity> Entities { get; set; } = [];
}