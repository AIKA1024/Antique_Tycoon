using System;
using System.Collections.Generic;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

public class PlayerMoveRequest(string playerUuid, List<string> path) : RequestBase
{
    public string PlayerUuid { get; set; } = playerUuid;
    public List<string> Path { get; set; }  = path;
}