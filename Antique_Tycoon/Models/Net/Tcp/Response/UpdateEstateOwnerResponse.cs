namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class UpdateEstateOwnerResponse:ITcpMessage
{
  public string Id { get; set; } = "";
  public long Timestamp { get; set; }
  public string EstateUuid { get; set; } = "";
  public string OwnerUuid { get; set; } = "";
}