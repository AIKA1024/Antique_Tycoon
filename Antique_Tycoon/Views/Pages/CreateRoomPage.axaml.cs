using Antique_Tycoon.ViewModels;
using Avalonia.Controls;

namespace Antique_Tycoon.Views.Pages;

public partial class CreateRoomPage : UserControl
{
  public CreateRoomPage()
  {
    InitializeComponent();
    DataContext = new CreateRoomPageViewModel();
  }
}