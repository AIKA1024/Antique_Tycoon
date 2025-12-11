namespace Antique_Tycoon.Models.Net.Tcp.Request;

public class UpdateEstateInfoRequest(string playerUuid,string estateUuid,int level = 0):IGameMessage
{
  public string Id { get; set; } = "";
  public long Timestamp { get; set; }
  public string PlayerUuid { get; set; } = playerUuid;
  public string EstateUuid { get; set; } = estateUuid;
  public int Level { get; set; } = level;
}