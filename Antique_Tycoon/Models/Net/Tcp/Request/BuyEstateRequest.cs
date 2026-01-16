namespace Antique_Tycoon.Models.Net.Tcp.Request;

[TcpMessage]
public class BuyEstateRequest : RequestBase
{
    public string EstateUuid { get; set; }
    public string PlayerUuid { get; set; }
    
    /// <summary>
    /// 是否要购买
    /// </summary>
    public bool IsConfirm { get; set; }

    public BuyEstateRequest()
    {
        EstateUuid = "";
        PlayerUuid = "";
        IsConfirm = false;
    }

    public BuyEstateRequest(string id,string playerUuid, string estateUuid)
    {
        Id = id;
        PlayerUuid = playerUuid;
        EstateUuid = estateUuid;
        IsConfirm = true;
    }
}