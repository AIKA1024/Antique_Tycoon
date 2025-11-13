using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using LibVLCSharp.Shared;
using Microsoft.Extensions.DependencyInjection;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Views.Controls;

public class ToggleLever : TemplatedControl
{
  private MediaPlayer _mediaPlayer = new MediaPlayer(App.Current.Services.GetRequiredService<LibVLC>());
  private static Media _flipDownSound = new(App.Current.Services.GetRequiredService<LibVLC>(),"Assets/SFX/MCButtonPressed.mp3");
  
  [GeneratedDirectProperty(true)]
  public bool IsPlaySound { get; set; }
}