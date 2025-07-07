using Antique_Tycoon.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Antique_Tycoon.Views.Pages;

public partial class HallPage : UserControl
{
  public HallPage()
  {
    InitializeComponent();
    DataContext = new HallPageViewModel();
  }
}