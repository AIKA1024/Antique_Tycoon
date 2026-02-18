using System;
using System.Collections.Generic;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

[TcpMessage]
public class PlayerMoveRequest(string playerUuid, List<string> path) : RequestBase
{
    public string PlayerUuid { get; set; } = playerUuid;
    public List<string> Path { get; set; }  = path;
}