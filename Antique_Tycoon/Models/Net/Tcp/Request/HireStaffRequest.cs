namespace Antique_Tycoon.Models.Net.Tcp.Request;

[TcpMessage]
public class HireStaffRequest:RequestBase
{
  public string StaffUuid { get; set; }

  public HireStaffRequest(string id,string staffUuid)
  {
    Id = id;
    StaffUuid = staffUuid;
  }
}