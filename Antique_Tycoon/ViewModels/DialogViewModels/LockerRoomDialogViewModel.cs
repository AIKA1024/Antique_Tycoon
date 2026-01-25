using System;
using Antique_Tycoon.Models;
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
  [ObservableProperty]
  public partial Player Player { get; set; } = App.Current.Services.GetRequiredService<GameManager>().LocalPlayer;

  [RelayCommand]
  private void Submit()
  {
    CloseDialog(Player);
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