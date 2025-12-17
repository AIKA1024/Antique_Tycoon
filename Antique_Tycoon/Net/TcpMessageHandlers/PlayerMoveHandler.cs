using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace Antique_Tycoon.Net.TcpMessageHandlers;

public class PlayerMoveHandler(GameRuleService gameRuleService,GameManager gameManager):ITcpMessageHandler
{
    public bool CanHandle(TcpMessageType messageType) =>  messageType == TcpMessageType.PlayerMoveRequest;

    public async Task HandleAsync(string json, TcpClient client)
    {
        if (JsonSerializer.Deserialize(json, AppJsonContext.Default.PlayerMoveRequest) is { } playerMoveRequest)
        {
            await gameManager.NetServerInstance.Broadcast(new PlayerMoveResponse(playerMoveRequest.PlayerUuid,playerMoveRequest.DestinationNodeUuid));
            WeakReferenceMessenger.Default.Send(new PlayerMoveMessage(playerMoveRequest.PlayerUuid,playerMoveRequest.DestinationNodeUuid));
            if (playerMoveRequest.IsEndTurn)
               await gameRuleService.AdvanceToNextPlayerTurnAsync();
        }
    }
}