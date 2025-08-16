using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Antique_Tycoon.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public partial class SelectMapDialogViewModel : DialogViewModelBase
{
  /// <summary>
  /// 给xaml预览提供的无参构造函数
  /// </summary>
  public SelectMapDialogViewModel()
  {
    
  }
  public SelectMapDialogViewModel(IEnumerable<Map> maps)
  {
    Maps = new ObservableCollection<Map>(maps);
  }
  [ObservableProperty] public partial ObservableCollection<Map> Maps { get; set; }
}