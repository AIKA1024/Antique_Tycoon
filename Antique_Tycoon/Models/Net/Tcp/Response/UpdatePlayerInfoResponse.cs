using System.Collections.Generic;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class UpdatePlayerInfoResponse(Player player) : ResponseBase, IHistoryRecord
{
  public Player Player { get; set; } = player;
  public List<LogSegment> LogSegments { get; set; } = [];
}