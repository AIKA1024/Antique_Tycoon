using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Timers;
using Antique_Tycoon.Net;
using Antique_Tycoon.Services;
using Antique_Tycoon.Utilities;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels;

public partial class DebugWindowViewModel : ObservableObject
{
  public ActionQueueService ActionQueueService { get; } =
    App.Current.Services.GetRequiredService<ActionQueueService>();

  public NetClient NetClient { get; } =
    App.Current.Services.GetRequiredService<NetClient>();

  private Timer _timer = new Timer(1000);

  public ObservableCollection<string> Logs { get; } = [];

#if DEBUG
  public int PendingRequestsCount => NetClient.PendingRequestsCount;

#endif

  public DebugWindowViewModel()
  {
#if DEBUG
    var writer = new AvalonConsoleWriter(line =>
    {
      Dispatcher.UIThread.Post(() =>
      {
        Logs.Add($"[{DateTime.Now:HH:mm:ss}] {line}");
        // 限制日志行数，防止内存溢出
        if (Logs.Count > 500) Logs.RemoveAt(0);
      });
    });
    Console.SetOut(writer);
    _timer.Elapsed += (_, _) => { OnPropertyChanged(nameof(PendingRequestsCount)); };
    _timer.Start();
#endif
  }
}