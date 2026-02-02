using System.Collections.Generic;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Entities;
using Antique_Tycoon.Models.Nodes;
using Antique_Tycoon.Services;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public partial class SaleAntiqueDialogViewModel : DialogViewModelBase<SaleAntiqueDetermination?>
{
  private readonly SoundService _soundService = App.Current.Services.GetRequiredService<SoundService>();
  public IList<ItemStack<Antique>> AntiqueStacks { get; }
  [ObservableProperty] public partial ItemStack<Antique>? SelectedStack { get; set; }

  private bool CanSell() => SelectedStack != null;

  public SaleAntiqueDialogViewModel(IList<ItemStack<Antique>> antiqueStacks)
  {
    AntiqueStacks = antiqueStacks;
    HorizontalAlignment = HorizontalAlignment.Stretch;
    VerticalAlignment = VerticalAlignment.Stretch;
  }

  partial void OnSelectedStackChanged(ItemStack<Antique>? value)
  {
    if (value != null)
    {
      _soundService.PlaySoundEffect("Assets/SFX/MCButtonPressed.mp3");
    }
  }

  [RelayCommand]
  private void Cancel()
  {
    CloseDialog(null);
  }

  [RelayCommand(CanExecute = nameof(CanSell))]
  private void SellAndUpgrade()
  {
    CloseDialog(new SaleAntiqueDetermination(SelectedStack!.Item, true));
  }

  [RelayCommand(CanExecute = nameof(CanSell))]
  private void SellAndHarvest()
  {
    CloseDialog(new SaleAntiqueDetermination(SelectedStack!.Item, false));
  }
}