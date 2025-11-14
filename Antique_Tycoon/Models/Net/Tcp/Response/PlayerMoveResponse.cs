namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class PlayerMoveResponse(string playerUuid, string destinationNodeUuid):ResponseBase
{
    public string PlayerUuid { get; set; } = playerUuid;
    public string DestinationNodeUuid { get; set; }  = destinationNodeUuid;
}