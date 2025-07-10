using System;
using System.Threading;
using Antique_Tycoon.Models;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Antique_Tycoon.ViewModels;

public partial class RoomPageViewModel: ViewModelBase
{
  private readonly CancellationTokenSource _cancellationTokenSource;
  public RoomPageViewModel(CancellationTokenSource cts)
  {
    _cancellationTokenSource = cts;
  }
  public AvaloniaList<Player> Players { get; set; } = [new() { Name = "lbw" }];
  public override void OnBacked()
  {
    base.OnBacked();
    _cancellationTokenSource.Cancel();
  }
}