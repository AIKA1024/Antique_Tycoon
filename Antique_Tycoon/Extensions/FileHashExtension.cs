using System;
using System.IO;
using System.Security.Cryptography;

namespace Antique_Tycoon.Extensions;

public static class FileHashExtension
{
  public static string ComputeFileHash(this string filePath)
  {
    using var sha256 = SHA256.Create();
    using var stream = File.OpenRead(filePath);
    byte[] hashBytes = sha256.ComputeHash(stream);

    // 转换为十六进制字符串
    return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
  }
}