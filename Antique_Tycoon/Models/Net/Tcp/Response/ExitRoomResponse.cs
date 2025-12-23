namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class ExitRoomResponse(string playerUuid):ResponseBase
{
    public string PlayerUuid { get; set; } = playerUuid;
}