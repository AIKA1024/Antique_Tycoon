using System.Collections.Generic;
using Antique_Tycoon.Models.Enums;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class GetAntiqueResultResponse:ResponseBase,IHistoryRecord
{
  private readonly string _antiqueName;
  private readonly string _playerName;
  public string AntiqueUuid { get; set; }
  public string PlayerUuid { get; set; }
  public string MineUuid { get; set; }

  public GetAntiqueResultResponse(string antiqueUuid,string antiqueName, string playerUuid,string playerName,string mineUuid,bool isSuccess)
  {
    _antiqueName = antiqueName;
    _playerName = playerName;
    AntiqueUuid = antiqueUuid;
    PlayerUuid = playerUuid;
    MineUuid = mineUuid;
    ResponseStatus = isSuccess?RequestResult.Success:RequestResult.Reject;
  }

  public List<LogSegment> GetLogSegments()
  {
    if (ResponseStatus == RequestResult.Success)
    {
      return [
        new LogSegment
        {
          Text = _playerName,
          Data = PlayerUuid,
          Type = InteractionType.PlayerName
        },
        new LogSegment
        {
          Text = " 获得了 "
        },
        new LogSegment
        {
          Text = _antiqueName,
          Data = AntiqueUuid,
          Type = InteractionType.Item
        }
      ];
    }

    return [
      new LogSegment
      {
        Text = _playerName,
        Data = PlayerUuid,
        Type = InteractionType.PlayerName
      },
      new LogSegment
      {
        Text = " 没能获得 "
      },
      new LogSegment
      {
        Text = _antiqueName,
        Data = AntiqueUuid,
        Type = InteractionType.Item
      }
    ];
  }
  
}