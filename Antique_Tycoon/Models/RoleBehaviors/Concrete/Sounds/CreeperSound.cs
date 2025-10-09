using System;
using Antique_Tycoon.Models.RoleBehaviors.Interfaces;
using LibVLCSharp.Shared;

namespace Antique_Tycoon.Models.RoleBehaviors.Concrete.Sounds;

public class CreeperSound(LibVLC libVlc) : ISoundStrategy
{
  private readonly MediaPlayer _sfxPlayer = new(libVlc);
  private readonly Media _happySound = new(libVlc, "Assets/SFX/Player/Creeper/HappySound.ogg");

  private readonly Media[] _unHappySounds =
  [
    new(libVlc, "Assets/SFX/Player/Creeper/UnhappySound1.ogg"),
    new(libVlc, "Assets/SFX/Player/Creeper/UnhappySound2.ogg"),
    new(libVlc, "Assets/SFX/Player/Creeper/UnhappySound3.ogg"),
    new(libVlc, "Assets/SFX/Player/Creeper/UnhappySound4.ogg"),
  ];

  public void PlayHappySound() => _sfxPlayer.Play(_happySound);
  public void PlayUnhappySound() => _sfxPlayer.Play(_unHappySounds[Random.Shared.Next(_unHappySounds.Length)]);
  public void PlayDeathSound() => _sfxPlayer.Play(new Media(libVlc, "Assets/SFX/Player/Creeper/Death.ogg"));
}