namespace Antique_Tycoon.Models.Net.Tcp.Response.GameAction;

[TcpMessage]
public class SaleAntiqueAction(string sellerUuid, string buyerUuid, string estateUuid) : ActionBase
{
  public string SellerUuid { get; set; } = sellerUuid;
  /// <summary>
  /// 空字符串代表银行
  /// </summary>
  public string BuyerUuid { get; set; } = buyerUuid;
  public string EstateUuid { get; set; } = estateUuid;
}