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
  private readonly Dictionary<DialogViewModelBase, TaskCompletionSource> _dialogTasks = [];
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
    var tcs = new TaskCompletionSource();
    _dialogTasks.Add(dialogViewModel, tcs);
    return tcs.Task;
  }

  public void CloseDialog(DialogViewModelBase dialogViewModel)
  {
    if (_dialogTasks.TryGetValue(dialogViewModel, out var value))
    {
      value.SetResult();
      _dialogTasks.Remove(dialogViewModel);
      _dialogs.Remove(dialogViewModel);
    }
  }
}