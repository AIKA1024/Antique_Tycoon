using Antique_Tycoon.Models;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Antique_Tycoon.Messages;

public class TurnStartMessage(Player value): ValueChangedMessage<Player>(value);