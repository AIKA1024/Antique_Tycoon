namespace Antique_Tycoon.Models.Net.Tcp;

public interface IGameMessage:ITcpMessage
{
  string PlayerUuid { get;  }
}