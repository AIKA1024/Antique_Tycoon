using System.Windows.Input;
using Antique_Tycoon.Models;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.PageViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Views.Widgets;

public partial class MapCard : UserControl
{
  public MapCard()
  {
    InitializeComponent();
  }
  
  [GeneratedDirectProperty]
  public partial ICommand Command { get; set; }
  
  [GeneratedDirectProperty]
  public partial object? CommandParameter { get; set; }
}