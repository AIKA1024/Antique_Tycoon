using System;
using Avalonia;
using Avalonia.Controls;
using LibVLCSharp.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Views.Controls;

public class McButton : Button
{
  private MediaPlayer _mediaPlayer;
  private static Media _pressedSound = new(App.Current.Services.GetRequiredService<LibVLC>(),"Assets/SFX/MCButtonPressed.mp3");
  // private static Media _pressedSound = new(App.Current.Services.GetRequiredService<LibVLC>(),"Assets/SFX/Sheep/HappySound1.ogg");
  public McButton()
  {
    _mediaPlayer = new MediaPlayer(App.Current.Services.GetRequiredService<LibVLC>());
  }
  // 注册 DirectProperty
  public static readonly DirectProperty<McButton, bool> IsPlaySoundProperty =
    AvaloniaProperty.RegisterDirect<McButton, bool>(
      nameof(IsPlaySound),   // 属性名
      o => o.IsPlaySound,    // Getter 委托
      (o, v) => o.IsPlaySound = v, // Setter 委托
      true); // 默认值

  // 公共属性
  public bool IsPlaySound
  {
    get;
    set => SetAndRaise(IsPlaySoundProperty, ref field, value);
  }

  protected override void OnClick()
  {
    base.OnClick();
    if (IsPlaySound)
      _mediaPlayer.Play(_pressedSound);
  }
}