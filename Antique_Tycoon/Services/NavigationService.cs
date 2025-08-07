using System;
using System.Collections.Generic;
using Antique_Tycoon.ViewModels;
using Antique_Tycoon.ViewModels.PageViewModels;
using Avalonia.Controls;

namespace Antique_Tycoon.Services;

public class NavigationService
{
  private readonly MainWindowViewModel _mainWindowViewModel;
  private readonly List<PageViewModelBase> _navigationHistory = [];

  public NavigationService(MainWindowViewModel viewModel)
  {
    _mainWindowViewModel = viewModel;
  }

  public void Navigation(PageViewModelBase vm)
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
    if (_mainWindowViewModel.CurrentPageViewModel is IDisposable needDisposeObj)
      needDisposeObj.Dispose();
    _mainWindowViewModel.CurrentPageViewModel = _navigationHistory[^1];
    _navigationHistory.RemoveAt(_navigationHistory.Count - 1);
  }
}