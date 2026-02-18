using System.Collections.Generic;
using System.Linq;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class PlayerMoveResponse(string playerUuid, List<string> path):ResponseBase
{
    public string PlayerUuid { get; set; } = playerUuid;
    public List<string> Path { get; set; }  = path;
}