using System;
using System.IO;
using System.Security.Cryptography;
using Avalonia.Media.Imaging;

namespace Antique_Tycoon.Extensions;

public static class BitmapExtension
{
  public static string GetGuid(this Bitmap bitmap)
  {
    using var ms = new MemoryStream();
    // 注意：这里的保存格式必须统一，否则同一张图保存为不同格式哈希会变
    bitmap.Save(ms); 
    byte[] bytes = ms.ToArray();

    using var md5 = MD5.Create();
    byte[] hashBytes = md5.ComputeHash(bytes);
    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
  }
}