using Antique_Tycoon.Models;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.PageViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Views.Widgets;

public partial class MapCard : UserControl
{
  public MapCard()
  {
    InitializeComponent();
  }

  private void Button_OnClick(object? sender, RoutedEventArgs e)
  {
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new MapEditPageViewModel(DataContext as Map));
  }
}