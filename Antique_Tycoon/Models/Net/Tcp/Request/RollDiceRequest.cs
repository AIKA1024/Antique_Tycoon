using System;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

[TcpMessage]
public class RollDiceRequest:RequestBase
{
  public RollDiceRequest(string id)
  {
    Id = id;
  }
}