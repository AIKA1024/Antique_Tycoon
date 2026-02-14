namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class HireStaffResponse:ResponseBase
{
  public string PlayerUuid { get; set; }
  public string StaffUuid { get; set; }
  public HireStaffResponse(string id,string playerUuid,string staffUuid,bool isSuccess)
  {
    Id = id;
    PlayerUuid = playerUuid;
    StaffUuid = staffUuid;
    ResponseStatus = isSuccess ? RequestResult.Success : RequestResult.Reject;
  }
}