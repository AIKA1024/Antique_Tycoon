using Antique_Tycoon.Models.Net.Tcp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Antique_Tycoon.Net;

public abstract class NetBase
{
    protected readonly Dictionary<string, FileDownloadContext> Downloads = new();
    private const int ChunkSize = 64 * 1024;
    public string DownloadPath { get; set; } = Environment.CurrentDirectory;
    private readonly SemaphoreSlim _sendLock = new(1, 1);
    public virtual TimeSpan DefaultTimeOuTimeSpan { get; set; } = TimeSpan.FromSeconds(30);

    protected async Task WriteStreamAsync(TcpClient client, byte[] data, CancellationToken cancellationToken)
    {
        await _sendLock.WaitAsync(cancellationToken);
        try
        {
            var stream = client.GetStream();
            await stream.WriteAsync(data, cancellationToken);
            await stream.FlushAsync(cancellationToken);
        }
        catch (Exception e) when (e is SocketException or InvalidOperationException or IOException
                                      or OperationCanceledException)
        {
            Console.WriteLine("发送失败，连接已断开");
            OnConnectionLost(client);
            throw;
        }
        finally
        {
            _sendLock.Release();
        }
    }

    protected async Task ReceiveLoopAsync(TcpClient client, CancellationToken cancellationToken = default)
    {
        var stream = client.GetStream();
        var buffer = new byte[8192]; // 网络读缓冲
        var memory = new MemoryStream(); // 存储未处理数据
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                int bytesRead;
                try
                {
                    bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                }
                catch (IOException)
                {
                    Console.WriteLine("读取流异常，连接可能已意外断开");
                    break; // 退出循环，执行清理逻辑
                }
                catch (SocketException)
                {
                    Console.WriteLine("Socket异常，连接已断开");
                    break;
                }

                if (bytesRead == 0)
                {
                    Console.WriteLine("对方主动关闭了连接");
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

                    if (messageLength < 2)
                    {
                        Console.WriteLine($"[严重警告] 收到非法数据包，声明长度过短 ({messageLength})，无法包含 MessageType。断开连接。");
                        throw new InvalidDataException($"非法数据包，包长 {messageLength} 过短。");
                    }

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
                                {
                                    Console.WriteLine($"ProcessMessageAsync failed: {t.Exception}");
                                    throw t.Exception;
                                }
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
        catch (Exception ex)
        {
            Console.WriteLine($"ReceiveLoop 发生严重错误: {ex}");
        }
        finally
        {
            // 触发断开事件，客户端可以在这里写自动重连逻辑，服务端可以在这里清理玩家
            OnConnectionLost(client);
        }
    }

    public byte[] PackMessage<T>(T message) where T : ITcpMessage
    {
        // var typeInfo =  GetTypeInfo(message);
        var typeInfo = TcpMessageRegistry.Get(message.GetType());
        var payload = JsonSerializer.SerializeToUtf8Bytes(message, typeInfo.jsonTypeInfo);
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

            await WriteStreamAsync(client, packet, CancellationToken.None);
            await Task.Yield();// 短暂地挂起当前发送文件的任务，给外面的心跳包和玩家操作去抢锁的机会。

            chunkIndex++;
        }
    }

    protected async Task ReceiveFileChunkAsync(
        string uuid,
        string fileName,
        int chunkIndex,
        int totalChunks,
        byte[] data)
    {
        string safeFileName = Path.GetFileName(fileName);
        string fullPath = Path.Combine(DownloadPath, safeFileName);

        FileDownloadContext ctx;
        lock (Downloads)
        {
            if (!Downloads.TryGetValue(fullPath, out ctx))
            {
                ctx = new FileDownloadContext(fullPath, totalChunks, ChunkSize);
                Downloads[fullPath] = ctx;
            }
        }

        await ctx.WriteChunkAsync(chunkIndex, data, 0, data.Length);

        if (ctx.IsCompleted)
        {
            ctx.Dispose();
            Console.WriteLine("触发ctx.Dispose");
            lock (Downloads)
            {
                Downloads.Remove(fullPath);
            }

            OnFileDownloadCompleted(uuid, fileName);
        }
    }

    protected virtual void OnFileDownloadCompleted(string uuid, string fileName)
    {
    }

    protected abstract Task ProcessMessageAsync(TcpMessageType tcpMessageType, string json, TcpClient client);

    protected virtual void OnConnectionLost(TcpClient client)
    {
        client.Close();
    }
}