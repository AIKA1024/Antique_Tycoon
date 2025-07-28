namespace Antique_Tycoon.Models.Node;

public partial class Estate:CanvasEntity
{
  public int Value { get; set; }
  public int Level{ get; set; }
  public Player? Owner { get; set; }
}