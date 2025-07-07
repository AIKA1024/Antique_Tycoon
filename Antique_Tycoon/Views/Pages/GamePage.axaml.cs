using System;
using Antique_Tycoon.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Antique_Tycoon.Views.Pages;

public partial class GamePage : UserControl
{
  private Cursor DragHand;
  
  public GamePage()
  {
    InitializeComponent();
    DataContext = new GamePageViewModel();
    var bitmap = new Bitmap(AssetLoader.Open(new Uri("avares://Antique_Tycoon/Assets/DragHand.png")));
    DragHand = new Cursor(bitmap, new PixelPoint());
  }
  
  private void InputElement_OnPointerPressed(object? sender, PointerPressedEventArgs e)
  {
    if (e.GetCurrentPoint(this).Properties.IsRightButtonPressed)
      Cursor = DragHand;
  }

  private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
  {
    Cursor = Cursor.Default;
  }
}