using System;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models;

public partial class Player : ObservableObject,IDisposable
{
  [ObservableProperty] string _name;
  [ObservableProperty] string _money;
  [ObservableProperty] private Bitmap _avatar;
  private AvaloniaList<Antique> Antiques { get; set; }
  public void Dispose()
  {
    throw new NotImplementedException();
  }
}