using System.Collections.Generic;
using Antique_Tycoon.Models.Enums;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class UpdateEstateInfoResponse(string ownerUuid,string estateUuid,int level = 1):ResponseBase, IHistoryRecord
{
  public string OwnerUuid { get; set; } = ownerUuid;
  public string EstateUuid { get; set; } = estateUuid;
  public int Level { get; set; } = level;
  public List<LogSegment> LogSegments =>
  [
    new()
    {
      Data = OwnerUuid,
      Type = InteractionType.PlayerName
    },
    new() { Text = string.IsNullOrEmpty(OwnerUuid)?" 当掉了 ":" 获得了 " },
    new()
    {
      Type = InteractionType.Estate,
      Data = EstateUuid
    }
  ];
}