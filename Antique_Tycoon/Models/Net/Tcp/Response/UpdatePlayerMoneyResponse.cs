using System.Collections.Generic;
using Antique_Tycoon.Models.Enums;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]//todo 感觉还是全部用UpdatePlayerInfoResponse吧，在LogSegments中描述改了什么就行了
public class UpdatePlayerMoneyResponse(string playerUuid, decimal delta, decimal total) : ResponseBase, IHistoryRecord
{
  public string PlayerUuid { get; set; } = playerUuid;
  public decimal Delta { get; set; } = delta;
  public decimal Total { get; set; } = total;
  public List<LogSegment> LogSegments { get; set; } = [];
}