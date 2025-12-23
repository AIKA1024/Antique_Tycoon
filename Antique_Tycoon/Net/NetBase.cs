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
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Net.Tcp.Response.GameAction;

namespace Antique_Tycoon.Net;

public abstract class NetBase
{
  private readonly Dictionary<string, FileDownloadContext> _downloads = new();
  private const int ChunkSize = 64 * 1024;
  public string DownloadPath { get; set; } = Environment.CurrentDirectory;

  protected async Task ReceiveLoopAsync(TcpClient client, CancellationToken cancellationToken = default)
  {
    var stream = client.GetStream();
    var buffer = new byte[8192]; // 网络读缓冲
    var memory = new MemoryStream(); // 存储未处理数据

    while (!cancellationToken.IsCancellationRequested)
    {
      int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
      if (bytesRead == 0)
      {
        Console.WriteLine("连接已关闭");
        break;
      }

      // 写入 MemoryStream
      memory.Position = memory.Length; // 移到末尾
      memory.Write(buffer, 0, bytesRead);
      memory.Position = 0; // 从头开始解析

      while (true)
      {
        if (memory.Length - memory.Position < 4)
          break; // 长度不足，等待更多数据

        // 读取包长（不包含外层4字节长度本身）
        byte[] lenBytes = new byte[4];
        await memory.ReadExactlyAsync(lenBytes, 0, 4, cancellationToken);
        int messageLength = BitConverter.ToInt32(lenBytes, 0);

        if (memory.Length - memory.Position < messageLength)
        {
          // 包体不完整，回退4字节长度
          memory.Position -= 4;
          break;
        }

        // 读取完整包体
        byte[] messageBytes = new byte[messageLength];
        await memory.ReadExactlyAsync(messageBytes, 0, messageLength, cancellationToken);

        // 解析 type
        TcpMessageType type = (TcpMessageType)BitConverter.ToUInt16(messageBytes, 0);

        if (type != TcpMessageType.HeartbeatMessage)
          Console.WriteLine($"收到消息 type={type}");

        switch (type)
        {
          case TcpMessageType.DownloadMapResponse:
            // 文件块处理，顺序、乱序都可
            var header = FilePacketHeader.Deserialize(messageBytes, out var headerSize);

            var data = new byte[header.DataLength];
            Array.Copy(messageBytes, headerSize, data, 0, header.DataLength);

            // 调用虚方法，传递必要字段
            _ = ReceiveFileChunkAsync(
              header.Uuid,
              header.FileName,
              header.ChunkIndex,
              header.TotalChunks,
              data
            ).ContinueWith(t =>
            {
              if (t.Exception != null)
                Console.WriteLine($"ReceiveFileChunkAsync failed: {t.Exception}");
            }, TaskContinuationOptions.OnlyOnFaulted);

            break;

          default:
            // 普通 JSON 消息
            var json = Encoding.UTF8.GetString(messageBytes, 2, messageBytes.Length - 2);
            _ = ProcessMessageAsync(type, json, client).ContinueWith(t =>
            {
              if (t.Exception != null)
                Console.WriteLine($"ProcessMessageAsync failed: {t.Exception}");
            }, TaskContinuationOptions.OnlyOnFaulted);
            break;
        }
      }

      // 保存剩余未解析数据
      if (memory.Position < memory.Length)
      {
        var leftover = memory.ToArray()[(int)memory.Position..];
        memory.SetLength(0);
        memory.Write(leftover, 0, leftover.Length);
      }
      else
      {
        memory.SetLength(0);
      }

      memory.Position = 0;
    }
  }

  public byte[] PackMessage<T>(T message) where T : ITcpMessage
  {
    var typeInfo = GetTypeInfo(message);
    var payload =
      JsonSerializer
        .SerializeToUtf8Bytes(message,
          typeInfo.JsonType);
    var bodyLength = 2 + payload.Length;
    var buffer = new byte[4 + bodyLength];

    BitConverter.GetBytes(bodyLength).CopyTo(buffer, 0);
    BitConverter.GetBytes((ushort)typeInfo.tcpMessageType).CopyTo(buffer, 4);
    payload.CopyTo(buffer, 6);

    return buffer;
  }

  public async Task SendFileAsync(Stream fileStream, string uuid, string fileName, TcpMessageType type,
    TcpClient client)
  {
    var buffer = new byte[ChunkSize];
    int bytesRead;
    int chunkIndex = 0;
    int totalChunks = (int)Math.Ceiling((double)fileStream.Length / ChunkSize);

    var netStream = client.GetStream();

    while ((bytesRead = await fileStream.ReadAsync(buffer)) > 0)
    {
      var header = new FilePacketHeader
      {
        Type = type,
        ChunkIndex = chunkIndex,
        TotalChunks = totalChunks,
        Uuid = uuid,
        FileName = fileName,
        DataLength = bytesRead
      };

      var headerBytes = header.Serialize();
      int bodyLength = headerBytes.Length + bytesRead;

      var packet = new byte[4 + bodyLength];
      BitConverter.GetBytes(bodyLength).CopyTo(packet, 0); // 包长
      headerBytes.CopyTo(packet, 4); // 包头
      Array.Copy(buffer, 0, packet, 4 + headerBytes.Length, bytesRead); // 数据

      await netStream.WriteAsync(packet);

      chunkIndex++;
    }
  }

  protected virtual async Task ReceiveFileChunkAsync(
    string uuid,
    string fileName,
    int chunkIndex,
    int totalChunks,
    byte[] data)
  {
    string safeFileName = Path.GetFileName(fileName);
    string fullPath = Path.Combine(DownloadPath, safeFileName);

    FileDownloadContext ctx;
    lock (_downloads)
    {
      if (!_downloads.TryGetValue(fullPath, out ctx))
      {
        ctx = new FileDownloadContext(fullPath, totalChunks, ChunkSize);
        _downloads[fullPath] = ctx;
      }
    }

    await ctx.WriteChunkAsync(chunkIndex, data, 0, data.Length);

    if (ctx.IsCompleted)
    {
      ctx.Dispose();
      lock (_downloads)
      {
        _downloads.Remove(fullPath);
      }

      Console.WriteLine($"✅ 文件 {safeFileName} (uuid={uuid}) 下载完成！");
    }
  }

  public JsonTypeInfo GetJsonTypeInfo(TcpMessageType type)
  {
    return type switch
    {
      // --- 对于所有 Request 类型，如果你使用了多态注解，统一返回 RequestBase ---
      TcpMessageType.JoinRoomRequest => AppJsonContext.Default.JoinRoomRequest,
        TcpMessageType.BuyEstateRequest => AppJsonContext.Default.BuyEstateRequest,
        TcpMessageType.ExitRoomRequest => AppJsonContext.Default.ExitRoomRequest,
        TcpMessageType.HeartbeatMessage => AppJsonContext.Default.HeartbeatMessage,
        TcpMessageType.PlayerMoveRequest => AppJsonContext.Default.PlayerMoveRequest,
        TcpMessageType.RollDiceRequest => AppJsonContext.Default.RollDiceRequest, 
        TcpMessageType.DownloadMapRequest => AppJsonContext.Default.DownloadMapRequest,

      // --- 对于 Response 类型，返回具体的类型 ---
      TcpMessageType.JoinRoomResponse => AppJsonContext.Default.JoinRoomResponse,
      TcpMessageType.UpdateRoomResponse => AppJsonContext.Default.UpdateRoomResponse,
      TcpMessageType.StartGameResponse => AppJsonContext.Default.StartGameResponse,
      TcpMessageType.RollDiceResponse => AppJsonContext.Default.RollDiceResponse,
      TcpMessageType.PlayerMoveResponse => AppJsonContext.Default.PlayerMoveResponse,
      TcpMessageType.DownloadMapResponse => AppJsonContext.Default.DownloadMapResponse,
      TcpMessageType.UpdateEstateInfoResponse => AppJsonContext.Default.UpdateEstateInfoResponse,
      TcpMessageType.TurnStartResponse => AppJsonContext.Default.TurnStartResponse,
      TcpMessageType.InitGameMessageResponse => AppJsonContext.Default.InitGameResponse,
      TcpMessageType.ExitRoomResponse => AppJsonContext.Default.ExitRoomResponse,

      _ => throw new NotSupportedException($"未定义消息类型 {type} 的 JSON 解析上下文")
    };
  }

  private static (JsonTypeInfo<T> JsonType, TcpMessageType tcpMessageType) GetTypeInfo<T>(T message)
    where T : ITcpMessage
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
      _ when type == typeof(DownloadMapRequest) => ((JsonTypeInfo<T>)(object)AppJsonContext.Default.DownloadMapRequest,
        TcpMessageType.DownloadMapRequest),
      _ when type == typeof(DownloadMapResponse) => (
        (JsonTypeInfo<T>)(object)AppJsonContext.Default.DownloadMapResponse,
        TcpMessageType.DownloadMapResponse),
      _ when type == typeof(TurnStartResponse) => (
        (JsonTypeInfo<T>)(object)AppJsonContext.Default.TurnStartResponse,
        TcpMessageType.TurnStartResponse),
      _ when type == typeof(RollDiceRequest) => (
        (JsonTypeInfo<T>)(object)AppJsonContext.Default.RollDiceRequest,
        TcpMessageType.RollDiceRequest),
      _ when type == typeof(RollDiceResponse) => (
        (JsonTypeInfo<T>)(object)AppJsonContext.Default.RollDiceResponse,
        TcpMessageType.RollDiceResponse),
      _ when type == typeof(InitGameResponse) => (
        (JsonTypeInfo<T>)(object)AppJsonContext.Default.InitGameResponse,
        TcpMessageType.InitGameMessageResponse),
      _ when type == typeof(PlayerMoveRequest) => (
        (JsonTypeInfo<T>)(object)AppJsonContext.Default.PlayerMoveRequest,
        TcpMessageType.PlayerMoveRequest),
      _ when type == typeof(PlayerMoveResponse) => (
        (JsonTypeInfo<T>)(object)AppJsonContext.Default.PlayerMoveResponse,
        TcpMessageType.PlayerMoveResponse),
      _ when type == typeof(BuyEstateRequest) => (
        (JsonTypeInfo<T>)(object)AppJsonContext.Default.BuyEstateRequest,
        TcpMessageType.BuyEstateRequest),
      _ when type == typeof(UpdateEstateInfoResponse) => (
        (JsonTypeInfo<T>)(object)AppJsonContext.Default.UpdateEstateInfoResponse,
        TcpMessageType.UpdateEstateInfoResponse),
      _ when type == typeof(UpdatePlayerInfoResponse) => (
        (JsonTypeInfo<T>)(object)AppJsonContext.Default.UpdatePlayerInfoResponse,
        TcpMessageType.UpdatePlayerInfoResponse),
      _ when type == typeof(BuyEstateAction) => (
        (JsonTypeInfo<T>)(object)AppJsonContext.Default.BuyEstateAction,
        TcpMessageType.BuyEstateAction),
      _ when type == typeof(ExitRoomResponse) => (
        (JsonTypeInfo<T>)(object)AppJsonContext.Default.ExitRoomResponse,
        TcpMessageType.ExitRoomResponse),
      // 更多类型...
      _ => throw new NotSupportedException($"类型 {typeof(T).Name} 未注册在 JSON 上下文中")
    };
  }

  protected abstract Task ProcessMessageAsync(TcpMessageType tcpMessageType, string json, TcpClient client);
}