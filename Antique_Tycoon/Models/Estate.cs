using Antique_Tycoon.Models.Cell;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models;

public partial class Estate:CanvasEntity
{
  public int Value { get; set; }
  public int Level{ get; set; }
  public Player? Owner { get; set; }
  public Bitmap? Cover { get; set; }
}