using System.Collections;
using System.Collections.Generic;
using Antique_Tycoon.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Antique_Tycoon.Messages;

public class UpdateRoomMessage(IEnumerable<Player> value) : ValueChangedMessage<IEnumerable<Player>>(value);