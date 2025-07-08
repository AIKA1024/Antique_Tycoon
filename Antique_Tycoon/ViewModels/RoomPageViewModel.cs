using System;
using Antique_Tycoon.Models;
using Avalonia.Collections;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Antique_Tycoon.ViewModels;

public partial class RoomPageViewModel: ViewModelBase
{
  public AvaloniaList<Player> Players { get; set; } = [new() { Name = "lbw" }];
}