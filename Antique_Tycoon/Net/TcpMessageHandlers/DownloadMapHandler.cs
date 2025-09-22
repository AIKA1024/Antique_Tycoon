using System;
using System.Net.Sockets;
using System.Text.Json;
using System.Threading.Tasks;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Services;

namespace Antique_Tycoon.Net.TcpMessageHandlers;

public class DownloadMapHandler(Lazy<NetServer> netServerLazy,MapFileService mapFileService):ITcpMessageHandler
{
  private NetServer NetServer => netServerLazy.Value;

  public bool CanHandle(TcpMessageType messageType) => messageType == TcpMessageType.DownloadMapRequest;

  public async Task HandleAsync(string json, TcpClient client)
  {
    if (JsonSerializer.Deserialize(json, AppJsonContext.Default.DownloadMapRequest) is { } downloadMapRequest)
    {
      if (mapFileService.GetMapByHash(downloadMapRequest.Hash) is { } map)
      {
        await NetServer.SendFileAsync(mapFileService.GetMapFileStream(map),
          downloadMapRequest.Id, $"{mapFileService.GetMapFileHash(map)}.zip", TcpMessageType.DownloadMapResponse,
          client);
      }
      else
        await NetServer.SendResponseAsync(
          new DownloadMapResponse { Id = downloadMapRequest.Id, ResponseStatus = RequestResult.Error }, client);
    }
  }
}