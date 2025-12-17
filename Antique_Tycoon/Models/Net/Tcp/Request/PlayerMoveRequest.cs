using System;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

public class PlayerMoveRequest : GameMessageRequest
{
    public string DestinationNodeUuid { get; set; }

    public PlayerMoveRequest(string playerUuid, string destinationNodeUuid)
    {
        PlayerUuid = playerUuid;
        DestinationNodeUuid =  destinationNodeUuid;
    }
}