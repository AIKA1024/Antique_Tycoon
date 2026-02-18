using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

[TcpMessage]
public class UpdatePlayerInfoResponse(Player player,string updateMessage):ResponseBase//todo 太广泛了，要具体化
{
  public Player Player { get; set; } = player;
  //描述这次更新了什么，ui要展示这个文本
  public string UpdateMessage{ get; set; } = updateMessage;
}