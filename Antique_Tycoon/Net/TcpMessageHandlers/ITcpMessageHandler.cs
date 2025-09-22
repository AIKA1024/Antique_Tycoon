using System.Net.Sockets;
using System.Threading.Tasks;
using Antique_Tycoon.Models.Net.Tcp;

namespace Antique_Tycoon.Net.TcpMessageHandlers;

public interface ITcpMessageHandler
{
  
  // 这个方法检查处理器是否能处理某种消息类型
  bool CanHandle(TcpMessageType messageType);

  // 这个方法执行具体的业务逻辑
  Task HandleAsync(string json, TcpClient client);
}