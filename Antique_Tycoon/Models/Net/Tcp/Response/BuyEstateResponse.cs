namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class BuyEstateResponse(int deductAmount) : ResponseBase
{
  public int DeductAmount { get; set; } = deductAmount;
}