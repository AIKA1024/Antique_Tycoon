using System.Collections.Generic;
using Antique_Tycoon.Models.Enums;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class HireStaffResponse : ResponseBase, IHistoryRecord
{
  public string PlayerUuid { get; set; }
  public string StaffUuid { get; set; }
  
  public bool IsSuccess { get; set; }

  public HireStaffResponse(string id, string playerUuid, string staffUuid, bool isSuccess)
  {
    Id = id;
    PlayerUuid = playerUuid;
    StaffUuid = staffUuid;
    IsSuccess = isSuccess;
  }

  public List<LogSegment> LogSegments =>
  [
    new LogSegment
    {
      Type = InteractionType.PlayerName,
      Data = PlayerUuid,
    },
    new LogSegment
    {
      Text = " 雇佣了 ",
    },
    new LogSegment
    {
      Type = InteractionType.Staff,
      Data = StaffUuid,
    }
  ];
}