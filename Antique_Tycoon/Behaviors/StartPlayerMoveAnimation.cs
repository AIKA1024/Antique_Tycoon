using System.Collections.Generic;
using System.Threading.Tasks;
using Antique_Tycoon.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Antique_Tycoon.Behaviors;

public class StartPlayerMoveAnimation(Player player, List<string> path) : AsyncRequestMessage<Task>
{
    public Player Player { get; set; } = player;
    public List<string> Path{ get; set; } = path;
}