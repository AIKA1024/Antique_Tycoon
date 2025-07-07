using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models;

public partial class Estate:ObservableObject
{
  public string Name { get; set; } = "";
  public int Value { get; set; }
  public int Level{ get; set; }
  public Player? Owner { get; set; }
}