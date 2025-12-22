using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Services;
using Avalonia.Utilities;
using CommunityToolkit.Mvvm.Messaging;

namespace Antique_Tycoon.Net.TcpMessageHandlers;

public class RollDiceHandler(GameManager gameManager)
  : ITcpMessageHandler
{
  public bool CanHandle(TcpMessageType messageType)
  {
    return messageType == TcpMessageType.RollDiceRequest;
  }

  public async Task HandleAsync(string json, TcpClient client)
  {
    if (JsonSerializer.Deserialize(json, AppJsonContext.Default.RollDiceRequest) is { } rollDiceRequest)
    {
      string clientPlayerUuid = gameManager.GetPlayerUuidByTcpClient(client);
      if (gameManager.CurrentTurnPlayer.Uuid == clientPlayerUuid)
      {
        int value = Random.Shared.Next(1, 7);
        var response = new RollDiceResponse(rollDiceRequest.Id, clientPlayerUuid, value);
        await gameManager.NetServerInstance.Broadcast(response);
        WeakReferenceMessenger.Default.Send(response);
      }
      else
      {
        var rollDiceResponse = new RollDiceResponse(rollDiceRequest.Id, clientPlayerUuid, 0)
          { ResponseStatus = RequestResult.Reject };
        await gameManager.NetServerInstance.SendResponseAsync(
          rollDiceResponse, client);
      }
    }
  }
}