using Antique_Tycoon.Models.Nodes;

namespace Antique_Tycoon.ViewModels.DetailViewModels;

public class SpawnPointDetailViewModel(SpawnPoint model): NodeDetailViewModel(model)
{
  public SpawnPoint SpawnPoint => (SpawnPoint)Model;
}