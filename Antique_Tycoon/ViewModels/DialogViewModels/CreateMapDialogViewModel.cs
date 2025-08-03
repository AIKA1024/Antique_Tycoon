using System.ComponentModel.DataAnnotations;
using Antique_Tycoon.Models;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.PageViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public partial class CreateMapDialogViewModel : DialogViewModelBase
{
  [ObservableProperty] [Required] private string _mapName = "";

  [RelayCommand]
  private void Submit()
  {
    ValidateAllProperties();
    if (!HasErrors)
      NavigateToMapEditPage();
  }

  private void NavigateToMapEditPage()
  {
    CloseDialog();
    App.Current.Services.GetRequiredService<NavigationService>()
      .Navigation(new MapEditPageViewModel(new Map { Name = MapName }));
  }
}