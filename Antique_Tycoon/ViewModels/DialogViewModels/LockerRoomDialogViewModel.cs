using System;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Configs;
using Antique_Tycoon.Models.Enums;
using Antique_Tycoon.Services;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Player = Antique_Tycoon.Models.Player;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public partial class LockerRoomDialogViewModel : DialogViewModelBase<Player?>
{
  private PersistenceService _persistenceService = App.Current.Services.GetRequiredService<PersistenceService>();

  [ObservableProperty] public partial Player Player { get; set; } = new();

  public LockerRoomDialogViewModel()
  {
    var localPlayer = App.Current.Services.GetRequiredService<GameManager>().LocalPlayer;
    Player.Name = localPlayer.Name;
    Player.Role = localPlayer.Role;
  }

  [RelayCommand]
  private void Submit()
  {
    CloseDialog(Player);
    var playerConfig = _persistenceService.GetConfig<PlayerConfig>();
    playerConfig.Name = Player.Name;
    playerConfig.PlayerRole = Player.Role;
    _persistenceService.SaveConfig<PlayerConfig>();
  }

  [RelayCommand]
  private void Cancel()
  {
    CloseDialog(null);
  }

  [RelayCommand]
  private void ChangeRole(PlayerRole role)
  {
    Player.Role = role;
    if (Random.Shared.Next(0, 2) == 0)
      App.Current.Services.GetRequiredService<RoleStrategyFactory>().GetSoundStrategy(role).PlayHappySound();
    else
      App.Current.Services.GetRequiredService<RoleStrategyFactory>().GetSoundStrategy(role).PlayUnhappySound();
  }
}