using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Models.Net.Tcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;

namespace Antique_Tycoon.Net;

public abstract class NetBase
{
  public abstract event Action<IEnumerable<Player>>? RoomInfoUpdated;
  protected async Task ReceiveLoopAsync(TcpClient client, CancellationToken cancellationToken = default)
  {
    var buffer = new byte[4096];
    var memory = new MemoryStream();
    var stream = client.GetStream();    
    
    while (!cancellationToken.IsCancellationRequested)
    {
      int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
      if (bytesRead == 0) break; // 连接断开

      memory.Write(buffer, 0, bytesRead);

      while (true)
      {
        memory.Position = 0;
        if (memory.Length < 4) break; // 长度不足以读取包长

        var lenBytes = new byte[4];
        await memory.ReadExactlyAsync(lenBytes, 0, 4, cancellationToken);
        int messageLength = BitConverter.ToInt32(lenBytes, 0);

        if (memory.Length - 4 < messageLength) break; // 包体没读完

        var messageBytes = new byte[messageLength];
        await memory.ReadExactlyAsync(messageBytes, 0, messageLength, cancellationToken);

        // === ✅ 新增逻辑：读取 typeId ===
        ushort typeId = BitConverter.ToUInt16(messageBytes, 0);
        var json = Encoding.UTF8.GetString(messageBytes, 2, messageBytes.Length - 2);

        // 处理消息（带 typeId）
        _ = ProcessMessageAsync((TcpMessageType)typeId, json, client).ContinueWith(t =>
        {
          if (t.Exception != null)
          {
            Console.WriteLine($"ProcessMessageAsync failed: {t.Exception}");
            throw t.Exception;
          }
        }, TaskContinuationOptions.OnlyOnFaulted);

        // === 清除已读部分 ===
        var remaining = memory.Length - memory.Position;
        var leftover = memory.GetBuffer().AsSpan((int)memory.Position, (int)remaining).ToArray();
        memory.SetLength(0);
        memory.Write(leftover);
      }
    }
  }

  public byte[] PackMessage<T>(T message) where T : ITcpMessage
  {
    var typeInfo = GetTypeInfo(message);
    var payload =
      JsonSerializer
        .SerializeToUtf8Bytes(message,typeInfo.JsonType); //todo 这里应该用原生成器，可以配一个 TypeInfo 分发器，根据TcpMessageType指定不同的JsonTypeInfo
    var bodyLength = 2 + payload.Length;
    var buffer = new byte[4 + bodyLength];

    BitConverter.GetBytes(bodyLength).CopyTo(buffer, 0);
    BitConverter.GetBytes((ushort)typeInfo.tcpMessageType).CopyTo(buffer, 4);
    payload.CopyTo(buffer, 6);

    return buffer;
  }

  private static (JsonTypeInfo<T> JsonType, TcpMessageType tcpMessageType) GetTypeInfo<T>(T message) where T : ITcpMessage
  {
    var type = message.GetType();
    return type switch
    {
      _ when type == typeof(JoinRoomRequest) => ((JsonTypeInfo<T>)(object)AppJsonContext.Default.JoinRoomRequest,
        TcpMessageType.JoinRoomRequest),
      _ when type == typeof(JoinRoomResponse) => ((JsonTypeInfo<T>)(object)AppJsonContext.Default.JoinRoomResponse,
        TcpMessageType.JoinRoomResponse),
      _ when type == typeof(UpdateRoomResponse) => ((JsonTypeInfo<T>)(object)AppJsonContext.Default.UpdateRoomResponse,
        TcpMessageType.UpdateRoomResponse),
      _ when type == typeof(HeartbeatMessage) => ((JsonTypeInfo<T>)(object)AppJsonContext.Default.HeartbeatMessage,
        TcpMessageType.HeartbeatMessage),
      _ when type == typeof(ExitRoomRequest) => ((JsonTypeInfo<T>)(object)AppJsonContext.Default.ExitRoomRequest,
        TcpMessageType.ExitRoomRequest),
      _ when type == typeof(StartGameResponse) => ((JsonTypeInfo<T>)(object)AppJsonContext.Default.StartGameResponse,
        TcpMessageType.StartGameResponse),
      // 更多类型...
      _ => throw new NotSupportedException($"类型 {typeof(T).Name} 未注册在 JSON 上下文中")
    };
  }

  protected abstract Task ProcessMessageAsync(TcpMessageType tcpMessageType, string json, TcpClient client);
}