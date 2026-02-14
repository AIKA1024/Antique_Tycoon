namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class GetAntiqueResultResponse:ResponseBase
{
  public string AntiqueUuid { get; set; }
  public string PlayerUuid { get; set; }
  public string MineUuid { get; set; }

  public GetAntiqueResultResponse(string antiqueUuid, string playerUuid,string mineUuid,bool isSuccess)
  {
    AntiqueUuid = antiqueUuid;
    PlayerUuid = playerUuid;
    MineUuid = mineUuid;
    ResponseStatus = isSuccess?RequestResult.Success:RequestResult.Reject;
  }
}