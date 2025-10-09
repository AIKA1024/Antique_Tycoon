using System;
using Antique_Tycoon.Models.RoleBehaviors.Interfaces;
using LibVLCSharp.Shared;

namespace Antique_Tycoon.Models.RoleBehaviors.Concrete.Sounds;

public class PigSound(LibVLC libVlc) : ISoundStrategy
{
  private readonly MediaPlayer _sfxPlayer = new(libVlc);
  private readonly Media[] _happySounds =
  [
    new(libVlc, "Assets/SFX/Player/Pig/HappySound1.ogg"),
    new(libVlc, "Assets/SFX/Player/Pig/HappySound2.ogg"),
    new(libVlc, "Assets/SFX/Player/Pig/HappySound3.ogg")
  ];
  public void PlayHappySound() => _sfxPlayer.Play(_happySounds[Random.Shared.Next(_happySounds.Length)]);
  public void PlayUnhappySound() => PlayHappySound();//猪居然没有收击音效
  public void PlayDeathSound() => _sfxPlayer.Play(new Media(libVlc, "Assets/SFX/Player/Pig/Death.ogg"));
}