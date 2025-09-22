using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Services;

namespace Antique_Tycoon.Net.TcpMessageHandlers;

public class ExitRoomHandler(Lazy<GameManager> gameManagerLazy):ITcpMessageHandler
{
  private GameManager GameManagerInstance => gameManagerLazy.Value;
  public bool CanHandle(TcpMessageType messageType) => messageType == TcpMessageType.ExitRoomRequest;

  public async Task HandleAsync(string json, TcpClient client)
  {
    if (JsonSerializer.Deserialize(json, AppJsonContext.Default.ExitRoomRequest) is { } exitRoomRequest)
      await GameManagerInstance.ReceiveExitRoomRequest(exitRoomRequest, client);
  }
}