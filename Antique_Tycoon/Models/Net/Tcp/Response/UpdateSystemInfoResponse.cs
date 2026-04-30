using System.Collections.Generic;
using Antique_Tycoon.Models.Entities;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class UpdateSystemInfoResponse:ResponseBase
{
  public List<Antique> AntiquesInventory { get; set; } = [];
}