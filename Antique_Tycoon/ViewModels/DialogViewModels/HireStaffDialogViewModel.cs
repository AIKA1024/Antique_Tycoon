using System.Collections.Generic;
using Antique_Tycoon.Models.Entities;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public partial class HireStaffDialogViewModel : DialogViewModelBase<IStaff?>
{
  private readonly SoundService _soundService = App.Current.Services.GetRequiredService<SoundService>();

  public IList<ItemStack<IStaff>> Staffs { get; set; }
  [ObservableProperty] public partial ItemStack<IStaff>? SelectedStaff { get; set; }

  public HireStaffDialogViewModel(IList<ItemStack<IStaff>> staffs)
  {
    Staffs = staffs;
  }

  partial void OnSelectedStaffChanged(ItemStack<IStaff>? value)
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

  [RelayCommand]
  private void Hire()
  {
    CloseDialog(SelectedStaff.Item);
  }
}