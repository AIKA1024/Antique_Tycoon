using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Nodes;
using Antique_Tycoon.Services;

namespace Antique_Tycoon.Net.TcpMessageHandlers;

public class BuyEstateHandler(GameManager gameManager) : ITcpMessageHandler
{
  public bool CanHandle(TcpMessageType messageType) => messageType == TcpMessageType.BuyEstateRequest;

  public async Task HandleAsync(string json, TcpClient client)
  {
    if (JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.BuyEstateRequest) is { } buyEstateRequest)//是否可以购买的逻辑已经在gamerule判断了
    {
      var estate = (Estate)gameManager.SelectedMap.EntitiesDict[buyEstateRequest.EstateUuid];
      var playerUuid = gameManager.GetPlayerUuidByTcpClient(client);
      await gameManager.NetServerInstance.SendResponseAsync(new BuyEstateResponse(estate.Value),client);//通知购买的客户端扣钱
      await gameManager.NetServerInstance.Broadcast(new UpdateEstateInfoResponse(playerUuid, estate.Uuid));//通知所以客户端更新地产信息
    }
  }
}