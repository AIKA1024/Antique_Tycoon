using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

[TcpMessage]
public class PlunderAntiqueRequest : RequestBase
{
  public string AntiqueUuid { get; }

  public PlunderAntiqueRequest(string id, string antiqueUuid)
  {
    Id = id;
    AntiqueUuid = antiqueUuid;
  }
}