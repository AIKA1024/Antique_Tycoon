using Avalonia.Media.Imaging;

namespace Antique_Tycoon.Models.Cell;

public partial class Estate:CanvasEntity
{
  public int Value { get; set; }
  public int Level{ get; set; }
  public Player? Owner { get; set; }
}