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
  
  /// <summary>
  /// 从视觉树中查找指定类型的第一个子元素（递归查找）
  /// </summary>
  /// <typeparam name="T">目标控件类型</typeparam>
  /// <param name="root">查找的根控件</param>
  /// <returns>找到的目标控件，未找到返回 null</returns>
  public static T? FindVisualChild<T>(this Visual root) where T : Visual
  {
    // 根控件本身就是目标类型，直接返回
    if (root is T target)
      return target;

    // 迭代根控件的所有视觉子元素（Avalonia 用 Visual.Children 替代 VisualTreeHelper.GetChildrenCount）
    foreach (var child in root.GetVisualChildren())
    {
      // 递归查找子元素的后代
      var descendant = child.FindVisualChild<T>();
      if (descendant != null)
        return descendant;
    }

    return null;
  }
}