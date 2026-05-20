using System;
using Antique_Tycoon.Services;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Utilities;

public enum SoundTriggerType
{
    PointerPressed, // 鼠标按下瞬间 (极速响应，使用 Tunnel 截胡)
    Tapped, // 完整点击手势 (推荐给普通按钮，防误触)
    PointerReleased // 鼠标松开瞬间
}

public class SoundAction : AvaloniaObject
{
    // ==========================================
    // 属性 1：音效名称
    // ==========================================
    public static readonly AttachedProperty<string?> ClickSoundProperty =
        AvaloniaProperty.RegisterAttached<SoundAction, Control, string?>("ClickSound");

    public static void SetClickSound(AvaloniaObject element, string? value)
        => element.SetValue(ClickSoundProperty, value);

    public static string? GetClickSound(AvaloniaObject element)
        => element.GetValue(ClickSoundProperty);

    // ==========================================
    // 属性 2：是否启用开关
    // ==========================================
    public static readonly AttachedProperty<bool> IsSoundEnabledProperty =
        AvaloniaProperty.RegisterAttached<SoundAction, Control, bool>("IsSoundEnabled", defaultValue: true);

    public static void SetIsSoundEnabled(AvaloniaObject element, bool value)
        => element.SetValue(IsSoundEnabledProperty, value);

    public static bool GetIsSoundEnabled(AvaloniaObject element)
        => element.GetValue(IsSoundEnabledProperty);

    // ==========================================
    // 属性 3：触发时机 (新增，默认给 PointerPressed，因为游戏开发最常用)
    // ==========================================
    public static readonly AttachedProperty<SoundTriggerType> TriggerTypeProperty =
        AvaloniaProperty.RegisterAttached<SoundAction, Control, SoundTriggerType>(
            "TriggerType", defaultValue: SoundTriggerType.PointerPressed);

    public static void SetTriggerType(AvaloniaObject element, SoundTriggerType value)
        => element.SetValue(TriggerTypeProperty, value);

    public static SoundTriggerType GetTriggerType(AvaloniaObject element)
        => element.GetValue(TriggerTypeProperty);


    // ==========================================
    // 核心事件路由与生命周期管理
    // ==========================================
    static SoundAction()
    {
        // 关键：音效名称 或 触发类型 发生改变时，都去重新配置事件
        ClickSoundProperty.Changed.AddClassHandler<Control>(OnConfigurationChanged);
        TriggerTypeProperty.Changed.AddClassHandler<Control>(OnConfigurationChanged);
    }

    private static void OnConfigurationChanged(Control control, AvaloniaPropertyChangedEventArgs args)
    {
        // 第一步：暴力卸载所有可能已经挂载的事件，防止重复触发或内存泄漏
        control.RemoveHandler(InputElement.PointerPressedEvent, Control_EventHandler);
        control.RemoveHandler(InputElement.TappedEvent, Control_EventHandler);
        control.RemoveHandler(InputElement.PointerReleasedEvent, Control_EventHandler);

        // 如果没有配置音效名字，直接结束（相当于取消了声音）
        string? soundName = GetClickSound(control);
        if (string.IsNullOrEmpty(soundName)) return;

        // 第二步：根据配置的类型，重新挂载对应的事件
        SoundTriggerType triggerType = GetTriggerType(control);

        switch (triggerType)
        {
            case SoundTriggerType.PointerPressed:
                // Pressed 必须用 Tunnel 隧道策略，以便截胡 TabItem 的内部状态改变
                control.AddHandler(InputElement.PointerPressedEvent, Control_EventHandler, RoutingStrategies.Tunnel);
                break;
            case SoundTriggerType.Tapped:
                // 普通手势用 Bubble 冒泡即可
                control.AddHandler(InputElement.TappedEvent, Control_EventHandler, RoutingStrategies.Bubble);
                break;
            case SoundTriggerType.PointerReleased:
                control.AddHandler(InputElement.PointerReleasedEvent, Control_EventHandler, RoutingStrategies.Bubble);
                break;
        }
    }

    // 统一的事件执行入口（复用播放逻辑）
    private static void Control_EventHandler(object? sender, RoutedEventArgs e)
    {
        if (Design.IsDesignMode) return;

        if (sender is Control ctrl)
        {
            if (!ctrl.IsEnabled) return;
            if (!GetIsSoundEnabled(ctrl)) return;

            string? soundName = GetClickSound(ctrl);
            if (string.IsNullOrEmpty(soundName)) return;

            if (Application.Current is not App app) return;

            var soundService = app.Services.GetRequiredService<SoundService>();
            soundService.PlaySoundEffect(soundName);
        }
    }
}