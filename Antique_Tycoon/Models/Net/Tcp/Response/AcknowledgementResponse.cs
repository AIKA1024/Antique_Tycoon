using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

/// <summary>
/// 仅用于表示服务器处理了请求
/// </summary>
[TcpMessage]
public class AcknowledgementResponse : ResponseBase
{
  public AcknowledgementResponse(string id)
  {
    Id = id;
  }
}