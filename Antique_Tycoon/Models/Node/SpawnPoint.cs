using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Node;

public partial class SpawnPoint : NodeModel
{
  public int Bonus
  {
    get;
    set => SetProperty(ref field, value);
  } = 2000;
}