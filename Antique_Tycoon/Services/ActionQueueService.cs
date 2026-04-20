using System;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Models;
using Avalonia.Threading;

namespace Antique_Tycoon.Services;

public class ActionQueueService
{
    private readonly ConcurrentQueue<ActionTaskItem> _actions = new();
    private readonly SemaphoreSlim _signal = new(0); // 用于通知有新任务
    public ObservableCollection<ActionTaskItem> VisibleTasks { get; } = new();

    public ActionQueueService()
    {
        _ = ProcessQueueAsync();
    }

    public void Enqueue(ActionTaskItem actionTaskItem)
    {
        // 1. 加入逻辑队列
        _actions.Enqueue(actionTaskItem);

        // 2. 同步到 UI 集合 (必须在 UI 线程)
        Dispatcher.UIThread.Post(() => VisibleTasks.Add(actionTaskItem));

        _signal.Release();
    }

    private async Task ProcessQueueAsync()
    {
        while (true)
        {
            await _signal.WaitAsync();

            if (_actions.TryDequeue(out var taskItem))
            {
                try
                {
                    taskItem.Status = "运行中";
                    await taskItem.Action();
                    taskItem.Status = "已完成";
                }
                catch (Exception ex)
                {
                    taskItem.Status = $"错误: {ex.Message}";
                }
                finally
                {
                    Dispatcher.UIThread.Post(() => { VisibleTasks.Remove(taskItem); });
                }
            }
        }
    }
}