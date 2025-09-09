using System;
using System.Text;
using Antique_Tycoon.Models.Net.Tcp;

namespace Antique_Tycoon.Net;

public class FilePacketHeader
{
  public TcpMessageType Type { get; set; }
  public int ChunkIndex { get; set; }
  public int TotalChunks { get; set; }
  public string Uuid { get; set; } = string.Empty; // 固定 36
  public string FileName { get; set; } = string.Empty;
  public int DataLength { get; set; }

  private const int UuidLength = 36;

  /// <summary>
  /// 序列化为字节数组（不含包长与数据）
  /// </summary>
  public byte[] Serialize()
  {
    var fileNameBytes = Encoding.UTF8.GetBytes(FileName);
    var header = new byte[2 + 4 + 4 + UuidLength + 4 + fileNameBytes.Length + 4];

    int offset = 0;

    BitConverter.GetBytes((ushort)Type).CopyTo(header, offset);
    offset += 2;

    BitConverter.GetBytes(ChunkIndex).CopyTo(header, offset);
    offset += 4;

    BitConverter.GetBytes(TotalChunks).CopyTo(header, offset);
    offset += 4;

    var uuidBytes = Encoding.UTF8.GetBytes(Uuid);
    if (uuidBytes.Length != UuidLength)
      throw new InvalidOperationException($"UUID must be {UuidLength} chars.");

    uuidBytes.CopyTo(header, offset);
    offset += UuidLength;

    BitConverter.GetBytes(fileNameBytes.Length).CopyTo(header, offset);
    offset += 4;

    fileNameBytes.CopyTo(header, offset);
    offset += fileNameBytes.Length;

    BitConverter.GetBytes(DataLength).CopyTo(header, offset);
    offset += 4;

    return header;
  }

  /// <summary>
  /// 从字节数组解析包头
  /// </summary>
  public static FilePacketHeader Deserialize(byte[] buffer, out int headerSize)
  {
    int offset = 0;

    var type = (TcpMessageType)BitConverter.ToUInt16(buffer, offset);
    offset += 2;

    int chunkIndex = BitConverter.ToInt32(buffer, offset);
    offset += 4;

    int totalChunks = BitConverter.ToInt32(buffer, offset);
    offset += 4;

    string uuid = Encoding.UTF8.GetString(buffer, offset, UuidLength);
    offset += UuidLength;

    int fileNameLength = BitConverter.ToInt32(buffer, offset);
    offset += 4;

    string fileName = Encoding.UTF8.GetString(buffer, offset, fileNameLength);
    offset += fileNameLength;

    int dataLength = BitConverter.ToInt32(buffer, offset);
    offset += 4;

    headerSize = offset;

    return new FilePacketHeader
    {
      Type = type,
      ChunkIndex = chunkIndex,
      TotalChunks = totalChunks,
      Uuid = uuid,
      FileName = fileName,
      DataLength = dataLength
    };
  }
}