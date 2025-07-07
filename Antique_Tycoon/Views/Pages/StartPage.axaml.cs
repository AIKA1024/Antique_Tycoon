using Antique_Tycoon.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Antique_Tycoon.Views.Pages;

public partial class StartPage : UserControl
{
  public StartPage()
  {
    InitializeComponent();
    DataContext = new StartPageViewModel();
  }
}