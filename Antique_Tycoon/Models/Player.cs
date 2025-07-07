using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models;

public partial class Player : ObservableObject
{
  [ObservableProperty] string _name;
  [ObservableProperty] string _money;
  private AvaloniaList<Antique> Antiques { get; set; }
}