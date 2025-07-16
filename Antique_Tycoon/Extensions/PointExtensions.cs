using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Antique_Tycoon.Extensions;

public static class PointExtensions
{
  public static Point SnapToGrid(this Point point, double gridSize)
  {
    return new Point(
      Math.Round(point.X / gridSize) * gridSize,
      Math.Round(point.Y / gridSize) * gridSize
    );
  }
}