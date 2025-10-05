using Antique_Tycoon.Models;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public partial class LockerRoomDialogViewModel:DialogViewModelBase<Player?>
{
  [ObservableProperty]
  public partial Player Player { get; set; } = new Player{IsHomeowner = true};

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
}