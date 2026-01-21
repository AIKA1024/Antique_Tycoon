using System.Collections.Generic;
using Antique_Tycoon.Models.Entities;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public partial class SaleAntiqueDialogViewModel : DialogViewModelBase<Antique?>
{
  public IList<Antique> Antiques { get; }
  [ObservableProperty] public partial Antique? SelectedAntique { get; set; }

  public SaleAntiqueDialogViewModel(IList<Antique> antiques)
  {
    Antiques =  antiques;
    HorizontalAlignment = HorizontalAlignment.Stretch;
    VerticalAlignment = VerticalAlignment.Stretch;
  }

  [RelayCommand]
  private void SelectAntique(Antique antique)
  {
    CloseDialog(antique);
  }
}