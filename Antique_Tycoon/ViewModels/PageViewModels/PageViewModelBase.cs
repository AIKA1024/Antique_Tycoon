using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public abstract class PageViewModelBase : ObservableObject
{
  /// <summary>
  /// 在此页面触发返回命令时
  /// </summary>
  public virtual void OnBacked()
  {
    
  }

  /// <summary>
  /// 页面切到此时触发
  /// </summary>
  public virtual void OnNavigatedTo()
  {
    
  }

  /// <summary>
  /// 切换到其他页面时
  /// </summary>
  public virtual void OnNavigatingFrom()
  {
    
  }
}