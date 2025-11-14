using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Services;

namespace Antique_Tycoon.Net.TcpMessageHandlers;

public class PlayerMoveMessageHandler(GameManager gameManager):ITcpMessageHandler
{
    public bool CanHandle(TcpMessageType messageType) => messageType == TcpMessageType.PlayerMoveRequest;

    public async Task HandleAsync(string json, TcpClient client)
    {
        if (JsonSerializer.Deserialize(json, AppJsonContext.Default.PlayerMoveRequest) is { } playerMoveRequest)
        {
            //小项目，就不做校验目的地是否合法了
            await gameManager.NetServerInstance.Broadcast(new PlayerMoveResponse(playerMoveRequest.PlayerUuid,playerMoveRequest.DestinationNodeUuid));
        }
    }
}