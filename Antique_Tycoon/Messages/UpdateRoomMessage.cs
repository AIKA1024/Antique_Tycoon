using System.Collections;
using System.Collections.Generic;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.RoleBehaviors;
using CommunityToolkit.Mvvm.Messaging.Messages;
using Player = Antique_Tycoon.Models.Player;

namespace Antique_Tycoon.Messages;

public class UpdateRoomMessage(IEnumerable<Player> value) : ValueChangedMessage<IEnumerable<Player>>(value);