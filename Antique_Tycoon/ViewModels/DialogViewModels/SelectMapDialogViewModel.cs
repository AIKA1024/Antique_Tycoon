using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public partial class SelectMapDialogViewModel : DialogViewModelBase
{
  [ObservableProperty] public partial ObservableCollection<Map> Maps { get; set; }

  public SelectMapDialogViewModel(IEnumerable<Map> maps)
  {
    Maps = new ObservableCollection<Map>(maps);
  }

  [RelayCommand]
  private void ChangeMap(Map map)
  {
    App.Current.Services.GetRequiredService<GameManager>().SelectedMap = map;
    CloseDialog();
  }
}