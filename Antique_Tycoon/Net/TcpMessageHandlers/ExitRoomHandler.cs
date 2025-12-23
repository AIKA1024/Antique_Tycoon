using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace Antique_Tycoon.Net.TcpMessageHandlers;

public class ExitRoomHandler(GameManager gameManager):ITcpMessageHandler
{
  public bool CanHandle(TcpMessageType messageType) => messageType == TcpMessageType.ExitRoomRequest;

  public async Task HandleAsync(string json, TcpClient client)
  {
    if (JsonSerializer.Deserialize(json, AppJsonContext.Default.ExitRoomRequest) is { } exitRoomRequest)
    {
      var exitRoomResponse = new ExitRoomResponse(exitRoomRequest.PlayerUuid);
      await gameManager.NetServerInstance.BroadcastExcept(exitRoomResponse,client);
      WeakReferenceMessenger.Default.Send(exitRoomResponse);
    }
  }
}