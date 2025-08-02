using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.ViewModels;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Avalonia.Collections;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Services;

public partial class DialogService : ObservableObject
{
  private AvaloniaList<DialogViewModelBase> _dialogs = [];
  private readonly Dictionary<DialogViewModelBase, TaskCompletionSource> _dialogTasks = [];
  [ObservableProperty] public partial DialogViewModelBase? CurrentDialogViewModel { get; private set; }

  public DialogService()
  {
    _dialogs.CollectionChanged += DialogsOnCollectionChanged;
  }

  private void DialogsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
  {
    CurrentDialogViewModel = _dialogs.Count != 0 ? _dialogs[0] : null;
  }

  public Task ShowDialogAsync(DialogViewModelBase dialogViewModel)
  {
    _dialogs.Insert(0, dialogViewModel);
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