namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class UpdatePlayerInfoResponse(Player player,string message):ResponseBase
{
  public Player ChangedPlayer { get; set; } = player;
  //描述这次更新了什么，ui要展示这个文本
  public string UpdateMessage{ get; set; } = message;
}