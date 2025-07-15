using Antique_Tycoon.ViewModels;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Cell;

public abstract partial class CanvasEntity : ObservableObject
{
  [ObservableProperty] private double _left;
  [ObservableProperty] private double _top;
  [ObservableProperty] private string _title;
}