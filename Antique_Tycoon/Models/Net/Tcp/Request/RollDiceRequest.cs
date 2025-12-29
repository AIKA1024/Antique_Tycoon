using System;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

public class RollDiceRequest:RequestBase
{
  public RollDiceRequest(string id)
  {
    Id = id;
  }
}