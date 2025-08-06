using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Node;

public partial class SpawnPoint : CanvasEntity
{
  public int Bonus
  {
    get;
    set => SetProperty(ref field, value);
  } = 2000;
}