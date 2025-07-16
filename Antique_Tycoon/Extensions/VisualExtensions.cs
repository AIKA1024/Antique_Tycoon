using System;
using Avalonia;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Antique_Tycoon.Extensions;

public static class VisualExtensions
{
  public static Point GetPointerPosition(this Visual visual,PointerEventArgs e)
  {
    var root = visual.GetVisualRoot() as Visual;
    if (root != null)
    {
      var transform = visual.TransformToVisual(root);
      if (transform != null)
      {
        var screenPoint = e.GetPosition(root);
        return transform.Value.Invert().Transform(screenPoint);
      }
      else
        throw new Exception("无法获取transform");
    }
    throw new Exception("无法获取GetVisualRoot");
  }
}