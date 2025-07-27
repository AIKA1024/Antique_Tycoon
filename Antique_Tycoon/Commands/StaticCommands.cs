using System;
using System.Windows.Input;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels;
using Antique_Tycoon.ViewModels.PageViewModels;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Commands;

public static class StaticCommands
{
  public static ICommand BackCommand { get; } = new RelayCommand<PageViewModelBase?>((vm) =>
  {
    App.Current.Services.GetRequiredService<NavigationService>().Back();
    vm?.OnBacked();
  }, _ => App.Current.Services.GetRequiredService<NavigationService>().IsCanBack());
}