using Antique_Tycoon.Models.Node;
using Avalonia.Collections;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models;

public partial class Map : ObservableObject
{
  [ObservableProperty] private string _name = string.Empty;
  [ObservableProperty] private Bitmap _cover;
  [ObservableProperty] private double _canvasHeight = 2000;
  [ObservableProperty] private double _canvasWidth = 3600;
  [ObservableProperty] private IBrush _canvasBackground = new SolidColorBrush(Color.Parse("#262626"));
  [ObservableProperty] private IBrush _nodeDefaultBackground = new SolidColorBrush(Color.Parse("#eccc68"));
  public AvaloniaList<CanvasEntity> Entities { get; set; } = [];
}