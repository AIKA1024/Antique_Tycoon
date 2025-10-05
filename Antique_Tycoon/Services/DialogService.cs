using Antique_Tycoon.ViewModels.DialogViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Antique_Tycoon.Services;

public partial class DialogService : ObservableObject
{
  private readonly ObservableCollection<DialogViewModelBase> _dialogs = [];
  private readonly Dictionary<DialogViewModelBase, object> _dialogTasks = [];
  [ObservableProperty] public partial DialogViewModelBase? CurrentDialogViewModel { get; private set; }

  public DialogService()
  {
    _dialogs.CollectionChanged += DialogsOnCollectionChanged;
  }

  private void DialogsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    CurrentDialogViewModel = _dialogs.Count != 0 ? _dialogs[^1] : null;
  }

  public Task ShowDialogAsync(DialogViewModelBase dialogViewModel)
  {
    _dialogs.Add(dialogViewModel);
    var tcs = new XTaskCompletionSource();
    _dialogTasks.Add(dialogViewModel, tcs);
    return tcs.Task;
  }

  public Task ShowDialogAsync(DialogViewModelBase dialogViewModel, Task task)
  {
    _dialogs.Add(dialogViewModel);
    _dialogTasks.Add(dialogViewModel, task);
    return task;
  }

  public Task<T?> ShowDialogAsync<T>(DialogViewModelBase<T?> dialogViewModel)
  {
    _dialogs.Add(dialogViewModel);
    var tcs = new XTaskCompletionSource<T>();
    _dialogTasks.Add(dialogViewModel, tcs);
    return tcs.Task;
  }

  /// <summary>
  /// 使用自定义的Task来等待窗口
  /// </summary>
  /// <param name="dialogViewModel">对话框视图模型</param>
  /// <param name="task">任务</param>
  /// <typeparam name="T">任务返回值</typeparam>
  /// <typeparam name="T1">视图模型返回值（已丢弃）</typeparam>
  /// <returns>任务</returns>
  public Task<T> ShowDialogAsync<T, T1>(DialogViewModelBase<T1> dialogViewModel, Task<T> task)
  {
    _dialogs.Add(dialogViewModel);
    _dialogTasks.Add(dialogViewModel, task);
    return task;
  }

  public void CloseDialogsAndClearResults(DialogViewModelBase dialogViewModel)
  {
    if (_dialogTasks.TryGetValue(dialogViewModel, out var obj) &&
        obj is ISupportsClearResult tcs)
    {
      tcs.ClearResult();
      _dialogTasks.Remove(dialogViewModel);
    }

    _dialogs.Remove(dialogViewModel);
  }

  public void CloseDialog<T>(DialogViewModelBase<T> dialogViewModel, T result)
  {
    if (_dialogTasks.TryGetValue(dialogViewModel, out var obj) &&
        obj is XTaskCompletionSource<T> tcs)
    {
      tcs.SetResult(result);
      _dialogTasks.Remove(dialogViewModel);
    }

    _dialogs.Remove(dialogViewModel);
  }

  public void CloseDialog(DialogViewModelBase dialogViewModel)
  {
    if (_dialogTasks.TryGetValue(dialogViewModel, out var obj) &&
        obj is XTaskCompletionSource tcs)
    {
      tcs.SetResult();
      _dialogTasks.Remove(dialogViewModel);
    }

    _dialogs.Remove(dialogViewModel);
  }
}