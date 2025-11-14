using System;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

public class PlayerMoveRequest(string playerUuid, string destinationNodeUuid) : IGameMessage
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    public string PlayerUuid { get; set; } = playerUuid;
    public string DestinationNodeUuid { get; set; } = destinationNodeUuid;
}