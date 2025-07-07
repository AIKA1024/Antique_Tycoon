using Antique_Tycoon.ViewModels;
using Avalonia.Controls;

namespace Antique_Tycoon.Services;

public class NavigationService
{
  private MainWindowViewModel _mainWindowViewModel;
  private UserControl? _previousPage;
  //todo 这样永远只能返回一次

  public NavigationService(MainWindowViewModel viewModel)
  {
    _mainWindowViewModel = viewModel;
  }

  public void Navigation(UserControl page)
  {
    _previousPage = _mainWindowViewModel.CurrentPage;
    _mainWindowViewModel.CurrentPage = page;
  }

  public bool IsCanBack()
  {
    return _previousPage != null;
  }

  public void Back()
  {
    _mainWindowViewModel.CurrentPage = _previousPage;
  }
}