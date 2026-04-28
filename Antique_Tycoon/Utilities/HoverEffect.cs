using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.VisualTree;

namespace Antique_Tycoon.Utilities; // 替换为你的实际命名空间

public class HoverEffect : AvaloniaObject
{
    public static readonly AttachedProperty<double> BrightnessFactorProperty =
        AvaloniaProperty.RegisterAttached<HoverEffect, Control, double>("BrightnessFactor", 0.0);

    public static double GetBrightnessFactor(Control element) => element.GetValue(BrightnessFactorProperty);
    public static void SetBrightnessFactor(Control element, double value) => element.SetValue(BrightnessFactorProperty, value);

    // 记录鼠标进入前的原始背景
    private static readonly AttachedProperty<IBrush?> OriginalBackgroundProperty =
        AvaloniaProperty.RegisterAttached<HoverEffect, Control, IBrush?>("OriginalBackground", null);

    // 记录鼠标进入前，背景是否显式设置了本地值 (Local Value)
    private static readonly AttachedProperty<bool> WasBackgroundLocalProperty =
        AvaloniaProperty.RegisterAttached<HoverEffect, Control, bool>("WasBackgroundLocal", false);

    static HoverEffect()
    {
        BrightnessFactorProperty.Changed.AddClassHandler<Control>(OnFactorChanged);
    }

    private static void OnFactorChanged(Control control, AvaloniaPropertyChangedEventArgs e)
    {
        var oldFactor = (double)e.OldValue!;
        var newFactor = (double)e.NewValue!;

        if (oldFactor == 0 && newFactor != 0)
        {
            control.PointerEntered += Control_PointerEntered;
            control.PointerExited += Control_PointerExited;
        }
        else if (oldFactor != 0 && newFactor == 0)
        {
            control.PointerEntered -= Control_PointerEntered;
            control.PointerExited -= Control_PointerExited;
        }
    }

    /// <summary>
    /// 核心辅助方法：因为 Avalonia 的 Control 没有统一的 Background 属性，
    /// 所以需要根据控件类型获取正确的 BackgroundProperty。
    /// </summary>
    private static AvaloniaProperty? GetBackgroundProperty(Control control)
    {
        return control switch
        {
            TemplatedControl => TemplatedControl.BackgroundProperty, // 涵盖 Button, TextBox, Slider 等等
            Panel => Panel.BackgroundProperty,                       // 涵盖 StackPanel, Grid, Canvas 等等
            Border => Border.BackgroundProperty,
            TextBlock => TextBlock.BackgroundProperty,
            _ => null
        };
    }

    private static void Control_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is not Control control) return;

        var factor = GetBrightnessFactor(control);
        if (factor == 0) return;

        // 获取当前控件对应的 BackgroundProperty
        var bgProperty = GetBackgroundProperty(control);
        if (bgProperty == null) return; // 如果该控件压根不支持 Background，则直接返回

        // 向上查找可用的颜色
        var bgBrush = FindBackground(control);
        if (bgBrush is ISolidColorBrush solidBrush)
        {
            // 1. 记录当前背景是否是本地设置的 (使用 IsSet 替代 ReadLocalValue)
            control.SetValue(WasBackgroundLocalProperty, control.IsSet(bgProperty));
            
            // 2. 记录当前的背景 Brush
            control.SetValue(OriginalBackgroundProperty, control.GetValue(bgProperty) as IBrush);

            // 3. 计算并设置新颜色
            var newColor = AdjustColor(solidBrush.Color, factor);
            control.SetValue(bgProperty, new SolidColorBrush(newColor));
        }
    }

    private static void Control_PointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is not Control control) return;

        var bgProperty = GetBackgroundProperty(control);
        if (bgProperty == null) return;

        // 恢复原来的背景
        bool wasLocal = control.GetValue(WasBackgroundLocalProperty);

        if (wasLocal)
        {
            // 如果原本显式写了 Background="Red"，恢复它
            var originalBrush = control.GetValue(OriginalBackgroundProperty);
            control.SetValue(bgProperty, originalBrush);
        }
        else
        {
            // 如果原本背景来自全局 Style，清除我们刚才赋的值，让 Style 重新接管
            control.ClearValue(bgProperty);
        }
    }

    private static IBrush? FindBackground(Visual? current)
    {
        while (current != null)
        {
            if (current is Control control)
            {
                var bgProp = GetBackgroundProperty(control);
                if (bgProp != null && control.GetValue(bgProp) is ISolidColorBrush solidBrush)
                {
                    if (solidBrush.Color.A > 0) // 排除完全透明的背景
                    {
                        return solidBrush;
                    }
                }
            }
            current = current.GetVisualParent();
        }
        return null;
    }

    private static Color AdjustColor(Color color, double factor)
    {
        factor = Math.Clamp(factor, -1.0, 1.0);

        double r = color.R;
        double g = color.G;
        double b = color.B;

        if (factor > 0)
        {
            r += (255 - r) * factor;
            g += (255 - g) * factor;
            b += (255 - b) * factor;
        }
        else if (factor < 0)
        {
            r *= (1 + factor);
            g *= (1 + factor);
            b *= (1 + factor);
        }

        return Color.FromArgb(color.A, (byte)r, (byte)g, (byte)b);
    }
}