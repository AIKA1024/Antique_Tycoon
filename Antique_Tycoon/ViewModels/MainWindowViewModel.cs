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
  public NavigationService NavigationService { get; } =
    App.Current.Services.GetRequiredService<NavigationService>();

  /// <summary>透传 NavigationService.CurrentPageViewModel，供 XAML 绑定</summary>
  public PageViewModelBase CurrentPageViewModel
  {
    get => NavigationService.CurrentPageViewModel;
    set => NavigationService.CurrentPageViewModel = value;
  }

  public DialogViewModelBase? DialogViewModel => DialogService.CurrentDialogViewModel;

  public DialogService DialogService { get; } =
    App.Current.Services.GetRequiredService<DialogService>();

  public MainWindowViewModel()
  {
    DialogService.DialogCollectionChanged += NotifyDialogViewModelChanged;
    NavigationService.PropertyChanged += (_, e) =>
    {
      if (e.PropertyName == nameof(NavigationService.CurrentPageViewModel))
        OnPropertyChanged(nameof(CurrentPageViewModel));
    };
  }

  private void NotifyDialogViewModelChanged()
  {
    OnPropertyChanged(nameof(DialogViewModel));
  }

  [RelayCommand]
  private void KeyPressed(Avalonia.Input.KeyGesture key)
  {
    WeakReferenceMessenger.Default.Send(new KeyPressedMessage(key));
  }

  [RelayCommand]
  private void CloseDialogByMaskTap(DialogViewModelBase dialogViewModel)
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
    var currentType = type;
    while (currentType != null && currentType != typeof(object))
    {
      if (currentType.IsGenericType &&
          currentType.GetGenericTypeDefinition() == typeof(DialogViewModelBase<>))
      {
        return true;
      }

      currentType = currentType.BaseType;
    }

    return false;
  }
}
