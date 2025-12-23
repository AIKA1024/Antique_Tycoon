using Avalonia;
using System;
using System.Threading.Tasks;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon;

sealed class Program
{
  // Initialization code. Don't use any Avalonia, third-party APIs or any
  // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
  // yet and stuff might break.
  [STAThread]
  public static void Main(string[] args)
  {
    AppDomain.CurrentDomain.UnhandledException += async (sender, e) =>
    {
      var ex = (Exception)e.ExceptionObject;
      await App.Current.Services.GetRequiredService<DialogService>().ShowDialogAsync(new MessageDialogViewModel
        { Title = "严重错误", Message = $"{ex.Message}\r\n程序即将关闭" });
      ExitApplication();
      // 此时应用通常即将崩溃，这里适合做最后的日志记录
    };
    // 2. 任务调度异常（未 await 的 Task）
    TaskScheduler.UnobservedTaskException += async (sender, e) =>
    {
      e.SetObserved(); // 标记为已观察，防止进程崩溃
      await App.Current.Services.GetRequiredService<DialogService>().ShowDialogAsync(new MessageDialogViewModel
        { Title = "严重错误", Message = $"{e.Exception.Message}\r\n程序即将关闭" });
      ExitApplication();
    };
    BuildAvaloniaApp()
      .StartWithClassicDesktopLifetime(args);
  } 

  // Avalonia configuration, don't remove; also used by visual designer.
  public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
      .UsePlatformDetect()
      .WithInterFont()
      .LogToTrace();
  
  private static void ExitApplication(int exitCode = 0)
  {
    // 获取当前应用的生命周期实例（跨平台通用）
    var appLifetime = Application.Current?.ApplicationLifetime;
    
    if (appLifetime == null)
    {
      // 兜底：生命周期未初始化时，强制终止进程
      Environment.Exit(exitCode);
      return;
    }

    // 桌面端（Windows/macOS/Linux）
    if (appLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
    {
      desktopLifetime.Shutdown(exitCode);
    }
    // 移动端/单窗口应用（如Android/iOS）
    else if (appLifetime is ISingleViewApplicationLifetime singleViewLifetime)
    {
      // 移动端通过终止进程实现退出（符合移动端行为）
      Environment.Exit(exitCode);
    }
    // 其他未知生命周期：兜底退出
    else
    {
      Environment.Exit(exitCode);
    }
  }
}