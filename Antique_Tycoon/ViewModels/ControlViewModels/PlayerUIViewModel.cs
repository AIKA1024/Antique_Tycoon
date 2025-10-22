using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Antique_Tycoon.Models;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.PageViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.ControlViewModels;

public partial class PlayerUiViewModel:PageViewModelBase
{
  [ObservableProperty] private bool _isVisible;
  [ObservableProperty] private Player _localPlayer;
  public ObservableCollection<Player> OtherPlayers { get; } = [];

  [RelayCommand]
  private async Task RollDiceAsync()
  {
    await App.Current.Services.GetRequiredService<GameRuleService>().RollDiceAsync();
    await Task.Delay(1000);
  }
}

