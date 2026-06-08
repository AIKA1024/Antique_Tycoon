using Avalonia;
using System;
using System.Threading.Tasks;
using Antique_Tycoon.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon;

sealed class Program
{
  [STAThread]
  public static void Main(string[] args)
  {
    // 1. 最终兜底：未被任何 try-catch 捕获的异常（应用即将崩溃）
    AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
    {
      var ex = (Exception)e.ExceptionObject;
      if (App.Current?.Services?.GetService(typeof(ExceptionHandlingService)) is ExceptionHandlingService exceptionService)
      {
        exceptionService.HandleException(ex, "未处理异常");
      }
      else
      {
        Console.WriteLine($"Critical Error before App Init: {ex}");
      }
    };

    // 2. 任务调度异常（未 await 的 Task 被 GC 时触发）
    TaskScheduler.UnobservedTaskException += (sender, e) =>
    {
      e.SetObserved(); // 标记为已观察，防止进程崩溃
      if (App.Current?.Services?.GetService(typeof(ExceptionHandlingService)) is ExceptionHandlingService exceptionService)
      {
        exceptionService.HandleException(e.Exception, "未观察任务异常");
      }
      else
      {
        Console.WriteLine($"UnobservedTaskException before App Init: {e.Exception}");
      }
    };

    BuildAvaloniaApp()
      .StartWithClassicDesktopLifetime(args);
  }

  public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
      .UsePlatformDetect()
      .WithInterFont()
      .LogToTrace();
}
