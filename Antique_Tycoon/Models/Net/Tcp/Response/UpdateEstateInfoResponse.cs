namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class UpdateEstateInfoResponse(string estateUuid,string ownerUuid,int level = 0):ResponseBase
{
  public string EstateUuid { get; set; } = estateUuid;
  public string OwnerUuid { get; set; } = ownerUuid;
  public int Level { get; set; } = level;
}