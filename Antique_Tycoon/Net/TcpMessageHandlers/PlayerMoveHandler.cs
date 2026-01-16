using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace Antique_Tycoon.Net.TcpMessageHandlers;

public class PlayerMoveHandler(GameManager gameManager):ITcpMessageHandler
{
    public bool CanHandle(TcpMessageType messageType) =>  messageType == TcpMessageType.PlayerMoveRequest;

    public async Task HandleAsync(string json, TcpClient client)
    {
        if (JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.PlayerMoveRequest) is { } playerMoveRequest)
        {
            if (playerMoveRequest.PlayerUuid != gameManager.CurrentTurnPlayer.Uuid)
                return;
            
            var response = new PlayerMoveResponse(playerMoveRequest.PlayerUuid, playerMoveRequest.Path)
                { Id = playerMoveRequest.Id };
            await gameManager.NetServerInstance.Broadcast(response);
            WeakReferenceMessenger.Default.Send(response);
        }
    }
}