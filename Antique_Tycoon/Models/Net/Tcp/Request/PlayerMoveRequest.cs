using System;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

public class PlayerMoveRequest(string playerUuid, string[] path) : RequestBase
{
    public string PlayerUuid { get; set; } = playerUuid;
    public string[] Path { get; set; }  = path;
}