using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Antique_Tycoon.Views.Windows;

public partial class MainWindow : Window
{
#if DEBUG
  private readonly DebugWindow _debugWindow;
#endif

  public MainWindow()
  {
    InitializeComponent();
#if DEBUG
    _debugWindow = new DebugWindow();
    _debugWindow.Show();
    Closing += (_, _) => _debugWindow.Close();
#endif
  }
}