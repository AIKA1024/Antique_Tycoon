using System.Collections.Generic;
using Antique_Tycoon.Models.Enums;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class UpdatePlayerMoneyResponse(string playerUuid, decimal delta, decimal total) : ResponseBase, IHistoryRecord
{
  public string PlayerUuid { get; set; } = playerUuid;
  public decimal Delta { get; set; } = delta;
  public decimal Total { get; set; } = total;
  public List<LogSegment> LogSegments { get; set; } = [];
}
//todo 不知道为什么路过出生点的消息客户端不显示历史记录