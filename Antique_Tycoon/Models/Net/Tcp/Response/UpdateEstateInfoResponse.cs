using System.Collections.Generic;
using Antique_Tycoon.Models.Enums;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class UpdateEstateInfoResponse(string ownerUuid,string ownerName,string estateUuid,string estateName,int level = 1):ResponseBase, IHistoryRecord
{
  public string OwnerUuid { get; set; } = ownerUuid;
  public string EstateUuid { get; set; } = estateUuid;
  public int Level { get; set; } = level;
  public List<LogSegment> GetLogSegments()
  {
    return
    [
      new LogSegment
      {
        Text = ownerName,
        Type = InteractionType.PlayerName
      },
      new LogSegment { Text = string.IsNullOrEmpty(OwnerUuid)?" 当掉了 ":" 获得了 " },
      new LogSegment
      {
        Text = estateName,
        Type = InteractionType.Location,
        Data = EstateUuid
      }
    ];
  }
}