namespace Antique_Tycoon.Models.Net.Tcp.Request;

public class UpdateEstateInfoRequest : GameMessageRequest
{
    public string EstateUuid { get; set; }
    public int Level { get; set; }

    public UpdateEstateInfoRequest(string playerUuid, string estateUuid, int level = 0)
    {
        PlayerUuid = playerUuid;
        EstateUuid = estateUuid;
        Level = level;
    }
}