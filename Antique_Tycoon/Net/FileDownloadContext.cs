using System;
using System.Collections;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace Antique_Tycoon.Net;

public class FileDownloadContext : IDisposable
{
  private readonly Lock _lock = new();
  private readonly BitArray _receivedChunks;
  private readonly int _chunkSize;

  public FileStream FileStream { get; }
  public int ExpectedChunks { get; }
  public int ReceivedChunks { get; private set; }

  public FileDownloadContext(string filePath, int expectedChunks, int chunkSize)
  {
    _chunkSize = chunkSize;
    ExpectedChunks = expectedChunks;
    _receivedChunks = new BitArray(expectedChunks, false);
    ReceivedChunks = 0;

    // 创建文件
    FileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
  }

  public async Task WriteChunkAsync(int chunkIndex, byte[] buffer, int offset, int count)
  {
    if (chunkIndex < 0 || chunkIndex >= ExpectedChunks)
      throw new ArgumentOutOfRangeException(nameof(chunkIndex), $"chunkIndex={chunkIndex} 超出范围 (0..{ExpectedChunks - 1})");

    // 避免重复写入同一个分块
    lock (_lock)
    {
      if (_receivedChunks[chunkIndex]) return;
      _receivedChunks[chunkIndex] = true;
      ReceivedChunks++;
    }

    // 计算文件偏移：固定 chunkSize
    long fileOffset = (long)chunkIndex * _chunkSize;

    // 异步写入
    await FileStream.WriteAsync(buffer.AsMemory(offset, count));
  }

  public bool IsCompleted
  {
    get
    {
      lock (_lock)
      {
        return ReceivedChunks >= ExpectedChunks && AllChunksReceived();
      }
    }
  }

  private bool AllChunksReceived()
  {
    for (int i = 0; i < ExpectedChunks; i++)
    {
      if (!_receivedChunks[i]) return false;
    }
    return true;
  }

  public void Dispose()
  {
    lock (_lock)
    {
      FileStream.Flush();
      FileStream.Dispose();
    }
  }
}