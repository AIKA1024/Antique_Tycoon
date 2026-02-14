using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace Antique_Tycoon.Services;

public class ActionQueueService
{
  private readonly ConcurrentQueue<Func<Task>> _actions = new();
  private readonly SemaphoreSlim _signal = new(0); // 用于通知有新任务
  private bool _isRunning;

  public ActionQueueService()
  {
    _ = ProcessQueueAsync();
  }

  // 入队方法：将逻辑包装成 Func<Task> 传入
  public void Enqueue(Func<Task> action)
  {
    _actions.Enqueue(action);
    _signal.Release(); // 唤醒处理循环
  }

  // 后台处理循环
  private async Task ProcessQueueAsync()
  {
    while (true)
    {
      await _signal.WaitAsync(); // 等待有任务入队

      if (_actions.TryDequeue(out var action))
      {
        try
        {
          // 核心：await action() 会等待动画/弹窗彻底结束（Task完成）
          // 才会进入下一次循环处理下一个消息
          Debug.WriteLine("开始一个任务");
          await action();
          Debug.WriteLine("处理了一个任务");
        }
        catch (Exception ex)
        {
          // 记录日志，防止一个任务报错导致整个队列卡死
          Console.WriteLine($"Action Queue Error: {ex}");
        }
      }
    }
  }
}