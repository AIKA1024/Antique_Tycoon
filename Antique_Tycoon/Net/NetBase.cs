using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Models.Net.Tcp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Avalonia;

namespace Antique_Tycoon.Net;

public abstract class NetBase
{
  private readonly Dictionary<string, FileDownloadContext> _downloads = new();
  private const int ChunkSize = 64 * 1024;
  public string DownloadPath { get; set; } = Environment.CurrentDirectory;
  public abstract event Action<IEnumerable<Player>>? RoomInfoUpdated;

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
        memory.Read(messageBytes, 0, messageLength);

        // 解析 type
        TcpMessageType type = (TcpMessageType)BitConverter.ToUInt16(messageBytes, 0);

        if (type != TcpMessageType.HeartbeatMessage)
          Console.WriteLine($"收到消息 type={type}");

        switch (type)
        {
          case TcpMessageType.DownloadMapResponse:
            // 文件块处理，顺序、乱序都可
            _ = ReceiveFileChunkAsync(messageBytes.AsSpan(2).ToArray()).ContinueWith(t =>
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

  public async Task SendFileAsync(Stream fileStream, TcpClient client, string fileName, TcpMessageType type)
  {
    var buffer = new byte[ChunkSize];
    int bytesRead;
    int chunkIndex = 0;
    int totalChunks = (int)Math.Ceiling((double)fileStream.Length / ChunkSize);

    var fileNameBytes = Encoding.UTF8.GetBytes(fileName);
    int fileNameLength = fileNameBytes.Length;

    var netStream = client.GetStream();

    while ((bytesRead = await fileStream.ReadAsync(buffer)) > 0)
    {
      int bodyLength = 2 + 4 + 4 + 4 + fileNameLength + 4 + bytesRead;
      var packet = new byte[4 + bodyLength];

      BitConverter.GetBytes(bodyLength).CopyTo(packet, 0);
      BitConverter.GetBytes((ushort)type).CopyTo(packet, 4);
      BitConverter.GetBytes(chunkIndex).CopyTo(packet, 6);
      BitConverter.GetBytes(totalChunks).CopyTo(packet, 10);
      BitConverter.GetBytes(fileNameLength).CopyTo(packet, 14);
      fileNameBytes.CopyTo(packet, 18);
      BitConverter.GetBytes(bytesRead).CopyTo(packet, 18 + fileNameLength);
      Array.Copy(buffer, 0, packet, 22 + fileNameLength, bytesRead);

      await netStream.WriteAsync(packet);
      chunkIndex++;
    }
  }

  private async Task ReceiveFileChunkAsync(byte[] messageBytes)
  {
    int offset = 0;

    // 1️⃣ 解析 chunkIndex
    int chunkIndex = BitConverter.ToInt32(messageBytes, offset);
    offset += 4;

    // 2️⃣ 解析 totalChunks
    int totalChunks = BitConverter.ToInt32(messageBytes, offset);
    offset += 4;

    // 3️⃣ 解析文件名长度
    int fileNameLength = BitConverter.ToInt32(messageBytes, offset);
    offset += 4;

    // 4️⃣ 解析文件名
    string fileName = Encoding.UTF8.GetString(messageBytes, offset, fileNameLength);
    offset += fileNameLength;

    // 5️⃣ 解析数据长度
    int dataLength = BitConverter.ToInt32(messageBytes, offset);
    offset += 4;

    // 6️⃣ 获取文件数据
    byte[] data = new byte[dataLength];
    Array.Copy(messageBytes, offset, data, 0, dataLength);

    string safeFileName = Path.GetFileName(fileName);
    string fullPath = Path.Combine(DownloadPath, safeFileName);

    FileDownloadContext ctx;
    lock (_downloads)
    {
      if (!_downloads.TryGetValue(fullPath, out ctx))
      {
        // 创建新的 FileDownloadContext，传入 totalChunks 和 ChunkSize
        ctx = new FileDownloadContext(fullPath, totalChunks, ChunkSize);
        _downloads[fullPath] = ctx;
      }
    }

    // 7️⃣ 异步写入 chunk（乱序安全）
    await ctx.WriteChunkAsync(chunkIndex, data, 0, data.Length);

    // 8️⃣ 检查是否完成
    if (ctx.IsCompleted)
    {
      ctx.Dispose();
      lock (_downloads)
      {
        _downloads.Remove(fullPath);
      }
      
      Console.WriteLine($"文件 {safeFileName} 下载完成！");
    }
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
      // 更多类型...
      _ => throw new NotSupportedException($"类型 {typeof(T).Name} 未注册在 JSON 上下文中")
    };
  }

  protected abstract Task ProcessMessageAsync(TcpMessageType tcpMessageType, string json, TcpClient client);
}