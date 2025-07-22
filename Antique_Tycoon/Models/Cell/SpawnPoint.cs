using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Cell;

public partial class SpawnPoint : CanvasEntity
{
  [ObservableProperty] private int _bonus = 2000;
}