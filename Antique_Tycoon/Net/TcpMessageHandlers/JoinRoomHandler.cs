using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Services;

namespace Antique_Tycoon.Net.TcpMessageHandlers;

public class JoinRoomHandler(Lazy<GameManager> gameManagerLazy) : ITcpMessageHandler
{
  private GameManager GameManagerInstance => gameManagerLazy.Value;

  // 通过构造函数注入依赖

  public bool CanHandle(TcpMessageType messageType) => messageType == TcpMessageType.JoinRoomRequest;

  public async Task HandleAsync(string json, TcpClient client)
  {
    if (JsonSerializer.Deserialize(json, AppJsonContext.Default.JoinRoomRequest) is { } joinRoomRequest)
    {
      await GameManagerInstance.ReceiveJoinRoomRequest(joinRoomRequest, client);
    }
  }
}