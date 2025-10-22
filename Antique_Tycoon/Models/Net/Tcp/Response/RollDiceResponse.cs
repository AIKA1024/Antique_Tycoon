namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class RollDiceResponse : ResponseBase
{
  public int DiceValue { get; set; }
  public string PlayerUuid { get; set; }

  public RollDiceResponse(string id,string playerUuid, int diceValue)
  {
    Id = id;
    PlayerUuid = playerUuid;
    DiceValue =  diceValue;
  }
}