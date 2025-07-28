using Avalonia.Media.Imaging;

namespace Antique_Tycoon.Models.Node;

public class Antique
{
  public string Name { get; set; } = "";
  public int Value { get; set; }
  public Bitmap Image { get; set; }
}