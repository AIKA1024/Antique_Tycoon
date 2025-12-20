namespace Antique_Tycoon.Models.Net.Tcp.Response.GameAction;

public class BuyEstateAction:ActionBase
{
  public string EstateUuid { get; set; }

  public BuyEstateAction(string estateUuid)
  {
    EstateUuid =  estateUuid;
  }
}