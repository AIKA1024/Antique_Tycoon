using System.Net.Sockets;
using Avalonia.Collections;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Models.Net;

public class Room
{
  public AvaloniaList<Player> Players { get; set; } = [App.Current.Services.GetRequiredService<Player>()];
  public int MaxPlayer { get; set; } = 5;
}