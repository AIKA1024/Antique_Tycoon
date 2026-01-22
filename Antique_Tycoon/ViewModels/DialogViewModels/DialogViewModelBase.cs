using System;
using Antique_Tycoon.Services;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Tmds.DBus.Protocol;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public abstract partial class DialogViewModelBase : ObservableValidator
{
  [ObservableProperty] public partial double MaxWidthPercent { get; set; } = .8f; // 0~1
  [ObservableProperty] public partial double MaxHeightPercent { get; set; } = .8f; // 0~1

  [ObservableProperty]
  public partial HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Center;

  [ObservableProperty] public partial VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Center;
  public bool IsLightDismissEnabled { get; set; } = true;

  public event Action? RequestClose;
  public void CloseDialog()
  {
    if (RequestClose is null)
      throw new Exception("没有服务负责这个对话框");
    RequestClose.Invoke();
  }
}

public abstract class DialogViewModelBase<T> : DialogViewModelBase
{
  public event Action<T>? RequestCloseWithResult;
  public void CloseDialog(T result)
  {
    if (RequestCloseWithResult is null)
      throw new Exception("没有服务负责这个对话框");
    RequestCloseWithResult.Invoke(result);
  }
}