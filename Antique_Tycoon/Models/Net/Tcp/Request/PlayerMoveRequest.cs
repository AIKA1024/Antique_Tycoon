using System;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

public class PlayerMoveRequest(string playerUuid, string destinationNodeUuid) : RequestBase
{
    public string DestinationNodeUuid { get; set; } = destinationNodeUuid;
    public string PlayerUuid { get; set; } = playerUuid;
}