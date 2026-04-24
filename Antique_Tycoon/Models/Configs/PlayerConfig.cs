using Antique_Tycoon.Models.Enums;

namespace Antique_Tycoon.Models.Configs;

public class PlayerConfig
{
  public string Name { get; set; } = "";
  public PlayerRole PlayerRole { get; set; }
}