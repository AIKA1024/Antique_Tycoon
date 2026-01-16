namespace Antique_Tycoon.Models.Net.Tcp.Request;

[TcpMessage]
public class SelectDestinationRequest:RequestBase
{
  public string DestinationUuid { get; set; }

  public SelectDestinationRequest(string id, string destinationUuid)
  {
    Id = id;
    DestinationUuid = destinationUuid;
  }
}