using System;
using Antique_Tycoon.Utilities;
using Antique_Tycoon.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Antique_Tycoon.Views.Windows;

public partial class DebugWindow : Window
{
    public DebugWindow()
    {
        InitializeComponent();
        DataContext = new DebugWindowViewModel();
    }
}