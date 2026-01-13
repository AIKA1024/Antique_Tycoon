namespace Antique_Tycoon.Models.Nodes;

public partial class SpawnPoint : NodeModel
{
  public int Bonus
  {
    get;
    set => SetProperty(ref field, value);
  } = 2000;
}