namespace Antique_Tycoon.Models.Net.Tcp.Request;

public class UpdateEstateOwnerRequest:IGameMessage
{
  public string Id { get; set; } = "";
  public long Timestamp { get; set; }
  public string PlayerUuid { get; set; } = "";
  public string EstateUuid { get; set; } = "";
}