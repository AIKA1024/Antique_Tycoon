using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Services;

namespace Antique_Tycoon.Net.TcpMessageHandlers;

public class DownloadMapHandler(GameManager gameManager):ITcpMessageHandler
{

  public bool CanHandle(TcpMessageType messageType) => messageType == TcpMessageType.DownloadMapRequest;

  public async Task HandleAsync(string json, TcpClient client)
  {
    if (JsonSerializer.Deserialize(json, AppJsonContext.Default.DownloadMapRequest) is { } downloadMapRequest)
      await gameManager.DownloadMap(downloadMapRequest, client);
  }
}