using System.Collections.Generic;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Response.GameAction;

[TcpMessage]
public class PlunderAntiqueAction(List<string> playerUuids):ActionBase
{
  public List<string> PlayerUuids { get; } = playerUuids;
}