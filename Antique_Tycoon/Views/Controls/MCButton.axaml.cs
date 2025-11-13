using System;
using Avalonia;
using Avalonia.Controls;
using LibVLCSharp.Shared;
using Microsoft.Extensions.DependencyInjection;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Views.Controls;

public partial class McButton : Button
{
  private MediaPlayer _mediaPlayer = new MediaPlayer(App.Current.Services.GetRequiredService<LibVLC>());
  private static Media _pressedSound = new(App.Current.Services.GetRequiredService<LibVLC>(),"Assets/SFX/MCButtonPressed.mp3");
  // private static Media _pressedSound = new(App.Current.Services.GetRequiredService<LibVLC>(),"Assets/SFX/Sheep/HappySound1.ogg");
  
  [GeneratedDirectProperty(true)]
  public partial bool IsPlaySound { get; set; }

  protected override void OnClick()
  {
    base.OnClick();
    if (IsPlaySound)
      _mediaPlayer.Play(_pressedSound);
  }
}