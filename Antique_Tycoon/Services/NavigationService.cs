using System.Collections.Generic;
using Antique_Tycoon.ViewModels;
using Avalonia.Controls;

namespace Antique_Tycoon.Services;

public class NavigationService
{
  private MainWindowViewModel _mainWindowViewModel;
  private List<ViewModelBase> _navigationHistory = new();
  //todo 这样永远只能返回一次

  public NavigationService(MainWindowViewModel viewModel)
  {
    _mainWindowViewModel = viewModel;
  }

  public void Navigation(ViewModelBase vm)
  {
    _navigationHistory.Add(_mainWindowViewModel.CurrentPageViewModel);
    _mainWindowViewModel.CurrentPageViewModel = vm;
  }

  public bool IsCanBack()
  {
    return _navigationHistory.Count != 0;
  }

  public void Back()
  {
    _mainWindowViewModel.CurrentPageViewModel = _navigationHistory[^1];
    _navigationHistory.RemoveAt(_navigationHistory.Count - 1);
  }
}