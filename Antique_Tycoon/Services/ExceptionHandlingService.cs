using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Services;

/// <summary>
/// 全局异常处理服务 — 捕获所有未被处理的异常并弹出对话框通知用户。
/// </summary>
public class ExceptionHandlingService
{
    private DialogService? _dialogService;

    private DialogService DialogService =>
        _dialogService ??= App.Current.Services.GetRequiredService<DialogService>();

    /// <summary>
    /// 处理异常：记录日志并弹窗提示。
    /// 在 UI 线程上安全调用，即使当前不在 UI 线程也会 Post 到 UI 线程。
    /// </summary>
    public void HandleException(Exception ex, string source = "")
    {
        // 始终记录到调试输出
        var sourceInfo = string.IsNullOrEmpty(source) ? "" : $"[{source}] ";
        Debug.Fail($"{sourceInfo}未处理异常: {ex}");

        // 弹窗必须在 UI 线程
        void ShowErrorDialog()
        {
            try
            {
                var message = ex is AggregateException agg
                    ? string.Join("\r\n", agg.InnerExceptions.Select(ie => ie.Message))
                    : ex.Message;

                // 非阻塞 fire-and-forget，避免在异常处理中阻塞 UI
                _ = DialogService.ShowDialogAsync(new MessageDialogViewModel
                {
                    Title = "错误",
                    Message = $"{sourceInfo}{message}",
                    IsLightDismissEnabled = false
                });
            }
            catch (Exception dialogEx)
            {
                // 弹窗本身也失败了，只能记录日志
                Debug.Fail($"无法显示错误弹窗: {dialogEx}");
            }
        }

        if (Dispatcher.UIThread.CheckAccess())
            ShowErrorDialog();
        else
            Dispatcher.UIThread.Post(ShowErrorDialog);
    }

    /// <summary>
    /// 安全的 fire-and-forget：启动异步任务，如果任务失败则通过 HandleException 弹窗。
    /// 保留调用方的 SynchronizationContext（不强制切换到线程池）。
    /// 用法: ExceptionHandlingService.FireAndForget(() => SomeAsyncMethod(), "来源描述")
    /// </summary>
    public static void FireAndForget(Func<Task> taskFactory, string source = "")
    {
        _ = FireAndForgetAsync(taskFactory, source);
    }

    private static async Task FireAndForgetAsync(Func<Task> taskFactory, string source)
    {
        try
        {
            await taskFactory();
        }
        catch (Exception ex)
        {
            var service = App.Current.Services?.GetService(typeof(ExceptionHandlingService)) as ExceptionHandlingService;
            if (service != null)
                service.HandleException(ex, source);
            else
                Debug.Fail($"[{source}] 未处理异常（ExceptionHandlingService 尚未初始化）: {ex}");
        }
    }

    /// <summary>
    /// 安全的 ContinueWith 替代：仅在任务失败时执行回调，不抛出未观察异常。
    /// </summary>
    public static void SafeContinueWith(Task task, Action<Exception> onFaulted)
    {
        task.ContinueWith(t =>
        {
            if (t.Exception != null)
            {
                // GetBaseException 在 AggregateException 时返回第一个内部异常
                var ex = t.Exception.GetBaseException();
                try
                {
                    onFaulted(ex);
                }
                catch (Exception callbackEx)
                {
                    Debug.Fail($"SafeContinueWith 回调异常: {callbackEx}");
                }
            }
        }, TaskContinuationOptions.OnlyOnFaulted);
    }
}
