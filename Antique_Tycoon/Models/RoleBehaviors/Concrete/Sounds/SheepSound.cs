using System;
using Antique_Tycoon.Models.RoleBehaviors.Interfaces;
using LibVLCSharp.Shared;

namespace Antique_Tycoon.Models.RoleBehaviors.Concrete.Sounds;

public class SheepSound(LibVLC libVlc) : ISoundStrategy
{
  private readonly MediaPlayer _sfxPlayer = new(libVlc);
  private readonly Media[] _happySounds =
  [
    new(libVlc, "Assets/SFX/Player/Sheep/HappySound1.ogg"),
    new(libVlc, "Assets/SFX/Player/Sheep/HappySound2.ogg"),
    new(libVlc, "Assets/SFX/Player/Sheep/HappySound3.ogg")
  ];

  private readonly Media[] _unHappySounds =
  [
    new(libVlc, "Assets/SFX/Player/Sheep/UnhappySound1.ogg"),
    new(libVlc, "Assets/SFX/Player/Sheep/UnhappySound2.ogg"),
    new(libVlc, "Assets/SFX/Player/Sheep/UnhappySound3.ogg")
  ];
  public void PlayHappySound() => _sfxPlayer.Play(_happySounds[Random.Shared.Next(_happySounds.Length)]);
  public void PlayUnhappySound() => _sfxPlayer.Play(_unHappySounds[Random.Shared.Next(_unHappySounds.Length)]);
  public void PlayDeathSound() => _sfxPlayer.Play(new Media(libVlc, "Assets/SFX/Player/Sheep/Death.ogg"));
}