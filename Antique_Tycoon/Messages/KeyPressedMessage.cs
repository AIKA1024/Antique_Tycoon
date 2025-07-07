using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Antique_Tycoon.Messages;

public class KeyPressedMessage: ValueChangedMessage<Avalonia.Input.KeyGesture>
{
  public KeyPressedMessage(KeyGesture value) : base(value)
  {
  }
}