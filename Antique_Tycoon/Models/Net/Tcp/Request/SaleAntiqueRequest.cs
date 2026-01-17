namespace Antique_Tycoon.Models.Net.Tcp.Request;

[TcpMessage]
public class SaleAntiqueRequest(string antiqueUuid, bool isUpgradeEstate) : RequestBase
{
  /// <summary>
  /// 如果为空字符串，代表什么也不卖
  /// </summary>
  public string AntiqueUuid { get; set; } = antiqueUuid;

  public bool IsUpgradeEstate { get; set; } = isUpgradeEstate;
}