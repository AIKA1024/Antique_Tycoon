using System;

namespace Antique_Tycoon.Models.Nodes;

public class TalentMarket:NodeModel
{
  public decimal Charge { get; set; } = 1000;

  public TalentMarket()
  {
    Title = "人才市场";
  }
}