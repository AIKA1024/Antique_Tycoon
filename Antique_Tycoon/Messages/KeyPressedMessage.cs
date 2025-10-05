using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Antique_Tycoon.Messages;

public class KeyPressedMessage(KeyGesture value) : ValueChangedMessage<KeyGesture>(value);