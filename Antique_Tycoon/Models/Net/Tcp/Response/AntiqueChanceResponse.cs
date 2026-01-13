namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class AntiqueChanceResponse:ResponseBase
{
  public string AntiqueUuid { get; set; }
  public string PlayerUuid { get; set; }
  public string MineUuid { get; set; }
  public string AnimationUuid { get; set; }
  public AntiqueChanceResponse(string antiqueUuid,string playerUuid,string mineUuid,string animationUuid)
  {
    AntiqueUuid = antiqueUuid;
    PlayerUuid = playerUuid;
    MineUuid = mineUuid;
    AnimationUuid = animationUuid;
  }
}