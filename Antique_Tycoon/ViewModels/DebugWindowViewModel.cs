using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
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

    public ObservableCollection<string> Logs { get; } = [];

    public DebugWindowViewModel()
    {
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
    }
}