using System;
using System.Windows.Input;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Commands;

public static class StaticCommands
{
  public static ICommand BackCommand { get; } = new RelayCommand(() =>
  {
    App.Current.Services.GetRequiredService<NavigationService>().Back();
  },() => App.Current.Services.GetRequiredService<NavigationService>().IsCanBack());
}