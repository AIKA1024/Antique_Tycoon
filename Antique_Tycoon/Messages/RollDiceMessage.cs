namespace Antique_Tycoon.Messages;

public class RollDiceMessage(string playerUuid, int diceValue,bool success = true)
{
  public string PlayerUuid = playerUuid;
  public int DiceValue = diceValue;
  public bool Success = success;
}