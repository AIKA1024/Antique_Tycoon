using System.Collections.Generic;
using Antique_Tycoon.Models.Entities;
using Antique_Tycoon.Models.Nodes;
using Antique_Tycoon.Services;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public partial class SaleAntiqueDialogViewModel : DialogViewModelBase<Antique?>//todo 返回值应该是一个新类型，还要表示是否收额外收益
{
  private readonly SoundService _soundService = App.Current.Services.GetRequiredService<SoundService>();
  public IList<AntiqueStack> AntiqueStacks { get; }
  [ObservableProperty] public partial AntiqueStack? SelectedStack { get; set; }
  
  private bool CanSell() => SelectedStack!= null;

  public SaleAntiqueDialogViewModel(IList<AntiqueStack> antiqueStacks)
  {
    AntiqueStacks =  antiqueStacks;
    HorizontalAlignment = HorizontalAlignment.Stretch;
    VerticalAlignment = VerticalAlignment.Stretch;
  }
  
  partial void OnSelectedStackChanged(AntiqueStack? value)
  {
    if (value != null)
    {
      _soundService.PlaySoundEffect("Assets/SFX/MCButtonPressed.mp3");
    }
  }
  
  [RelayCommand(CanExecute = nameof(CanSell))]
  private void SellAndUpgrade()
  {
    CloseDialog(SelectedStack!.Item);
  }

  [RelayCommand(CanExecute = nameof(CanSell))]
  private void SellAndHarvest()
  {
    CloseDialog(SelectedStack!.Item);
  }
}