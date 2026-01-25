namespace Antique_Tycoon.Models.Nodes;

public partial class SpawnPoint : NodeModel
{
  public decimal Bonus
  {
    get;
    set => SetProperty(ref field, value);
  } = 2000;
}