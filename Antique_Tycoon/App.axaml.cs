using System;
using System.IO;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using System.Threading.Tasks;
using Antique_Tycoon.Models;
using Antique_Tycoon.Net;
using Antique_Tycoon.Net.TcpMessageHandlers;
using Antique_Tycoon.Services;
using Avalonia.Markup.Xaml;
using Antique_Tycoon.ViewModels;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Antique_Tycoon.Views;
using Antique_Tycoon.Views.Windows;
using Avalonia.Controls;
using Avalonia.Threading;
using LibVLCSharp.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon;

public partial class App : Application
{
  public const int DefaultPort = 13437;

  public IServiceProvider Services { get; private set; }
  public new static App Current => (App)Application.Current!;
  public string MapPath { get; } = Path.Join("..", "Maps");
  public string DownloadMapPath { get; } = Path.Join("..", "TempDownloadPath");

  public override void Initialize()
  {
    AvaloniaXamlLoader.Load(this);
    var gameManager = Services.GetRequiredService<GameManager>();
    gameManager.Initialize();
    
    Services.GetRequiredService<GameRuleService>();// 启动gameRule todo 后面有多种规则后，需要按需实例化
    
  }

  public App()
  {
    Services = ConfigureServices();
    Directory.CreateDirectory(MapPath);
    Directory.CreateDirectory(DownloadMapPath);
  }
  
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

  private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
  {
    try
    {
      // 标记异常为已观察，避免进程崩溃
      e.SetObserved();
      var ex = e.Exception.Flatten().InnerException;
      Console.WriteLine($"未处理的异步任务异常：{ex.Message}\r\n{ex.StackTrace}");

      // 核心：切换到UI线程执行弹窗（避免跨线程访问）
      var task = Dispatcher.UIThread.InvokeAsync(async () =>
      {
        try
        {
          // 确保DialogService已初始化
          var dialogService = Services.GetRequiredService<DialogService>();
          await dialogService.ShowDialogAsync(new MessageDialogViewModel
          {
            Title = "严重错误",
            Message = $"{ex.Message}\r\n程序即将关闭",
            IsShowCancelButton = false // 错误弹窗不允许取消
          });
        }
        catch (Exception dialogEx)
        {
          // 弹窗失败时降级输出
          Console.WriteLine($"弹窗显示失败：{dialogEx.Message}");
        }
        finally
        {
          // 无论弹窗是否成功，最终执行退出
          ExitApplication();
        }
      });

      // 等待UI线程操作完成（避免进程提前退出）
      task.Wait();
    }
    catch (Exception handlerEx)
    {
      // 捕获事件处理程序自身的异常，避免崩溃
      Console.WriteLine($"异常处理程序出错：{handlerEx.Message}");
      // 兜底退出
      Environment.Exit(1);
    }
  }

  public override void OnFrameworkInitializationCompleted()
  {
    if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
    {
      // Avoid duplicate validations from both Avalonia and the CommunityToolkit. 
      // More info: https://docs.avaloniaui.net/docs/guides/development-guides/data-validation#manage-validationplugins
      DisableAvaloniaDataAnnotationValidation();
      desktop.MainWindow = Services.GetRequiredService<MainWindow>();
    }

    base.OnFrameworkInitializationCompleted();
    TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
  }

  private IServiceProvider ConfigureServices()
  {
    var services = new ServiceCollection();
    services.AddSingleton<MainWindow>(sp => new MainWindow
      { DataContext = sp.GetRequiredService<MainWindowViewModel>() });
    services.AddSingleton(new LibVLC("--no-video"));
    services.AddSingleton<MainWindowViewModel>();
    services.AddSingleton<NavigationService>(sp => new NavigationService(sp.GetRequiredService<MainWindowViewModel>()));
    services.AddSingleton<MapFileService>();
    services.AddSingleton<DialogService>();
    services.AddSingleton<GameManager>();
    services.AddSingleton<GameRuleService>();
    services.AddSingleton<RoleStrategyFactory>();
    services.AddTransient<ITcpMessageHandler, JoinRoomHandler>();
    services.AddTransient<ITcpMessageHandler, ExitRoomHandler>();
    services.AddTransient<ITcpMessageHandler, DownloadMapHandler>();
    services.AddTransient<ITcpMessageHandler, RollDiceHandler>();
    services.AddTransient<ITcpMessageHandler, PlayerMoveHandler>();
    services.AddSingleton<NetClient>(sp => new NetClient(sp.GetRequiredService<GameManager>(), DownloadMapPath));
    services.AddSingleton(sp => new Lazy<NetClient>(sp.GetRequiredService<NetClient>));
    services.AddSingleton<NetServer>(sp =>
      new NetServer(sp.GetServices<ITcpMessageHandler>(), DownloadMapPath)); // 这样注册才合理，NetClient不规范
    services.AddSingleton(sp => new Lazy<NetServer>(sp.GetRequiredService<NetServer>));
    services.AddSingleton(sp => TopLevel.GetTopLevel(sp.GetRequiredService<MainWindow>())!);
    return services.BuildServiceProvider();
  }

  private void DisableAvaloniaDataAnnotationValidation()
  {
    // Get an array of plugins to remove
    var dataValidationPluginsToRemove =
      BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

    // remove each entry found
    foreach (var plugin in dataValidationPluginsToRemove)
    {
      BindingPlugins.DataValidators.Remove(plugin);
    }
  }
}