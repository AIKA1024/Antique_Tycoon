namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class AntiqueChanceResponse:ResponseBase
{
  public string AntiqueUuid { get; set; }
  public string PlayerUuid { get; set; }
  public AntiqueChanceResponse(string antiqueUuid,string playerUuid)
  {
    AntiqueUuid = antiqueUuid;
    PlayerUuid = playerUuid;
  }
}