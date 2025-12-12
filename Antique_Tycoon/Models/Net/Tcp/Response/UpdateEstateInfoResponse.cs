namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class UpdateEstateInfoResponse(string ownerUuid,string estateUuid,int level = 1):ResponseBase
{
  public string OwnerUuid { get; set; } = ownerUuid;
  public string EstateUuid { get; set; } = estateUuid;
  public int Level { get; set; } = level;
}