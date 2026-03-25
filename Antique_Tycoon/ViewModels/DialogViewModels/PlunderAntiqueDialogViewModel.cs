using System.Collections.Generic;
using System.Linq;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Entities;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public partial class PlunderAntiqueDialogViewModel:DialogViewModelBase<Antique?>
{
  public List<Player> Players { get; }
  [ObservableProperty] public partial Player? SelectedPlayer { get; set; }
  [ObservableProperty] public partial List<ItemStack<Antique>> SelectedPlayerAntiqueStack { get; set; } = [];
  [ObservableProperty] public partial ItemStack<Antique>? SelectedStack { get; set; }

  public PlunderAntiqueDialogViewModel(List<Player> players)
  {
    Players = players;
    HorizontalAlignment = HorizontalAlignment.Stretch;
    VerticalAlignment = VerticalAlignment.Stretch;
  }

  partial void OnSelectedPlayerChanged(Player? value)
  {
    if (value == null)
      return;
    SelectedPlayerAntiqueStack = value.Antiques.GroupBy(a => a.Index)
      .Select(group => new ItemStack<Antique>(group.First(), group.Count())).ToList();
  }
  
  [RelayCommand]
  private void Cancel()
  {
    CloseDialog(null);
  }
  
  [RelayCommand]
  private void Confirm(ItemStack<Antique> antiqueStack)
  {
    CloseDialog(antiqueStack.Item);
  }
}