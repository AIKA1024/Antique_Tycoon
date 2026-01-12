namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class GetAntiqueResultResponse:ResponseBase
{
  public string AntiqueUuid { get; set; }
  public string PlayerUuid { get; set; }
  public bool IsSuccess { get; set; }

  public GetAntiqueResultResponse(string antiqueUuid, string playerUuid,bool isSuccess)
  {
    AntiqueUuid = antiqueUuid;
    PlayerUuid = playerUuid;
    IsSuccess = isSuccess;
  }
}