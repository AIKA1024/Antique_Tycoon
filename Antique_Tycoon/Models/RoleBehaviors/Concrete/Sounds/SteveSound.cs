using Antique_Tycoon.Models.RoleBehaviors.Interfaces;
using LibVLCSharp.Shared;

namespace Antique_Tycoon.Models.RoleBehaviors.Concrete.Sounds;

public class SteveSound(LibVLC libVlc) : ISoundStrategy
{
  private readonly MediaPlayer _sfxPlayer = new(libVlc);
  private readonly Media _happySound = new(libVlc, "Assets/SFX/Player/Steve/HappySound.ogg");
  private readonly Media _unHappySound = new(libVlc, "Assets/SFX/Player/Steve/UnhappySound.ogg");

  public void PlayHappySound() => _sfxPlayer.Play(_happySound);
  public void PlayUnhappySound() => _sfxPlayer.Play(_unHappySound);
  public void PlayDeathSound() => _sfxPlayer.Play(new Media(libVlc, "Assets/SFX/Player/Steve/Death.ogg"));
}