using Antique_Tycoon.Models.Entities;

namespace Antique_Tycoon.Models;

public class SaleAntiqueDetermination(Antique antique, bool needUpgrade)
{
  public Antique Antique { get; set; } = antique;
  public bool NeedUpgrade {get; set;} = needUpgrade;
}