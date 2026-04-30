using System.Collections.Generic;
using Antique_Tycoon.Models.Enums;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class GetAntiqueResultResponse(string antiqueUuid, string playerUuid, string mineUuid, bool isSuccess)
  : ResponseBase, IHistoryRecord
{
  public string AntiqueUuid { get; set; } = antiqueUuid;
  public string PlayerUuid { get; set; } = playerUuid;
  public string MineUuid { get; set; } = mineUuid;

  public bool IsSuccess { get; set; } = isSuccess;

  public List<LogSegment> LogSegments { get; set; } = [];
}