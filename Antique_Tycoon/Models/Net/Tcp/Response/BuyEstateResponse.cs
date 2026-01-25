namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class BuyEstateResponse(decimal deductAmount) : ResponseBase
{
  public decimal DeductAmount { get; set; } = deductAmount;
}