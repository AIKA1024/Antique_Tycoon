namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class UpdateEstateOwnerResponse:ResponseBase
{
  public string EstateUuid { get; set; } = "";
  public string OwnerUuid { get; set; } = "";
}