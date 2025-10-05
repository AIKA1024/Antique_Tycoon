using Antique_Tycoon.Models;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class GamePageViewModel : PageViewModelBase
{
  [ObservableProperty] private Map _map;
  private GameManager  _gameManager = App.Current.Services.GetRequiredService<GameManager>();
  public GamePageViewModel(Map map)
  {
    Map = map;
    foreach (var player in _gameManager.Players)
      Map.SpawnNode.PlayersHere.Add(player);
  }
}