using System;
using System.Collections.Generic;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace Antique_Tycoon.Utilities;

public static class ImageHelper
{
  private static readonly Dictionary<string, Bitmap> Dictionary = [];

  public static Bitmap GetBitmap(string assetPath)
  {
    Dictionary.TryGetValue(assetPath, out Bitmap? bitmap);
    bitmap ??= new Bitmap(AssetLoader.Open(new Uri(assetPath)));
    Dictionary[assetPath] = bitmap;
    return bitmap;
  }

  public static bool ReleaseBitmap(string assetPath) =>Dictionary.Remove(assetPath);
}