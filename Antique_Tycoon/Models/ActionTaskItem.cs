using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models;

public partial class ActionTaskItem:ObservableObject
{
    [ObservableProperty]
    public partial string Name { get; set; }
    [ObservableProperty]
    public partial string Status { get; set; } = "未启动";
    public Func<Task> Action { get; set; }

    public ActionTaskItem(string name, Func<Task> action)
    {
        Name = name;
        Action = action;
    }
}