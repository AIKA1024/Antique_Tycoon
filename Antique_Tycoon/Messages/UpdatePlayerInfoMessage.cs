using Antique_Tycoon.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Antique_Tycoon.Messages;

public class UpdatePlayerInfoMessage(Player value) : ValueChangedMessage<Player>(value);