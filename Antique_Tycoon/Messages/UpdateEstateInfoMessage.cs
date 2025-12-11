namespace Antique_Tycoon.Messages;

public class UpdateEstateInfoMessage(string playerUuid,string estateUuid,int level)
{
    public string PlayerUuid { get; set; } = playerUuid;
    public string EstateUuid { get; set; } = estateUuid;
    public int Level { get; set; } =  level;
}