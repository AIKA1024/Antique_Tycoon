using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Node;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Antique_Tycoon.Messages;

public class EstateBoughtMessage(Player player, Estate estate)
{
  public Player Player { get; } = player;
  public Estate Estate { get; } = estate;
}