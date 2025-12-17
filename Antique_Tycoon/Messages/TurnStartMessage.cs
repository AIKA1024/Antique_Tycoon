using CommunityToolkit.Mvvm.Messaging.Messages;
using Player = Antique_Tycoon.Models.Player;

namespace Antique_Tycoon.Messages;

public class TurnStartMessage(string value): ValueChangedMessage<string>(value);