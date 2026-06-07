using System;
using System.Collections.Generic;
using Antique_Tycoon.ViewModels.PageViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Services;

public partial class NavigationService : ObservableObject
{
  private readonly List<PageViewModelBase> _navigationHistory = [];

  [ObservableProperty] private bool _isTransitionReversed;

  [ObservableProperty] private PageViewModelBase _currentPageViewModel = new StartPageViewModel();

  public void Navigation(PageViewModelBase vm)
  {
    IsTransitionReversed = false;
    _navigationHistory.Add(CurrentPageViewModel);
    CurrentPageViewModel.OnNavigatingFrom();
    CurrentPageViewModel = vm;
    vm.OnNavigatedTo();
  }

  public bool IsCanBack()
  {
    return _navigationHistory.Count != 0;
  }

  public void Back()
  {
    IsTransitionReversed = true;
    if (CurrentPageViewModel is IDisposable needDisposeObj)
      needDisposeObj.Dispose();
    CurrentPageViewModel.OnNavigatingFrom();
    CurrentPageViewModel = _navigationHistory[^1];
    CurrentPageViewModel.OnNavigatedTo();
    _navigationHistory.RemoveAt(_navigationHistory.Count - 1);
  }
}
