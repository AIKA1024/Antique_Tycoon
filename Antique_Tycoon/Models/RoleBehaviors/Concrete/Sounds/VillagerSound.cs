using System;
using Antique_Tycoon.Models.RoleBehaviors.Interfaces;
using LibVLCSharp.Shared;

namespace Antique_Tycoon.Models.RoleBehaviors.Concrete.Sounds;

public class VillagerSound(LibVLC libVlc) : ISoundStrategy
{
  private readonly MediaPlayer _sfxPlayer = new(libVlc);

  private readonly Media[] _happySounds =
  [
    new(libVlc, "Assets/SFX/Player/Villager/HappySound1.ogg"),
    new(libVlc, "Assets/SFX/Player/Villager/HappySound2.ogg"),
    new(libVlc, "Assets/SFX/Player/Villager/HappySound3.ogg")
  ];

  private readonly Media[] _unHappySounds =
  [
    new(libVlc, "Assets/SFX/Player/Villager/UnhappySound1.ogg"),
    new(libVlc, "Assets/SFX/Player/Villager/UnhappySound2.ogg"),
    new(libVlc, "Assets/SFX/Player/Villager/UnhappySound3.ogg")
  ];

  public void PlayHappySound() => _sfxPlayer.Play(_happySounds[Random.Shared.Next(_happySounds.Length)]);
  public void PlayUnhappySound() => _sfxPlayer.Play(_unHappySounds[Random.Shared.Next(_unHappySounds.Length)]);

  public void PlayDeathSound() => _sfxPlayer.Play(new Media(libVlc, "Assets/SFX/Player/Villager/Death.ogg"));
}