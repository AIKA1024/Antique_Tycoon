using System;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Antique_Tycoon.ViewModels.PageViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels;

public partial class MainWindowViewModel : PageViewModelBase
{
  [ObservableProperty] private PageViewModelBase _currentPageViewModel = new StartPageViewModel();

  public DialogService DialogService { get; } =
    App.Current.Services.GetRequiredService<DialogService>();

  [RelayCommand]
  private void KeyPressed(Avalonia.Input.KeyGesture key)
  {
    WeakReferenceMessenger.Default.Send(new KeyPressedMessage(key));
  }

  // [RelayCommand]
  // private void CloseMask()
  // {
  //   var currentDialogViewModel = App.Current.Services.GetRequiredService<DialogService>().CurrentDialogViewModel;
  //   WeakReferenceMessenger.Default.Send(new DialogCompletedMessage(currentDialogViewModel));
  // }

  [RelayCommand]
  private void CloseMaskByMeshTap(DialogViewModelBase dialogViewModel)
  {
    if (dialogViewModel is { IsLightDismissEnabled : false })
      return;

    if (IsDerivedFromGenericDialogViewModelBase(dialogViewModel.GetType()))
      App.Current.Services.GetRequiredService<DialogService>().CloseDialogsAndClearResults(dialogViewModel);
    else
      App.Current.Services.GetRequiredService<DialogService>().CloseDialog(dialogViewModel);
  }

  private bool IsDerivedFromGenericDialogViewModelBase(Type type)
  {
    // 递归检查继承链
    var currentType = type;
    while (currentType != null && currentType != typeof(object))
    {
      // 检查当前类型是否是泛型类型，且泛型定义是 DialogViewModelBase<>
      if (currentType.IsGenericType &&
          currentType.GetGenericTypeDefinition() == typeof(DialogViewModelBase<>))
      {
        return true;
      }

      // 继续检查父类
      currentType = currentType.BaseType;
    }

    return false;
  }
}