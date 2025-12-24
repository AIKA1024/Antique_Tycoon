namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class PlayerMoveResponse(string playerUuid, string[] path):ResponseBase
{
    public string PlayerUuid { get; set; } = playerUuid;
    public string[] Path { get; set; }  = path;
}