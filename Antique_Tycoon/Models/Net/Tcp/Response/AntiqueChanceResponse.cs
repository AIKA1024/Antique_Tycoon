namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class AntiqueChanceResponse:ResponseBase
{
  public string AntiqueUuid { get; set; }
  public string PlayerUuid { get; set; }
  public string MineUuid { get; set; }
  public AntiqueChanceResponse(string antiqueUuid,string playerUuid,string mineUuid)
  {
    AntiqueUuid = antiqueUuid;
    PlayerUuid = playerUuid;
    MineUuid = mineUuid;
  }
}