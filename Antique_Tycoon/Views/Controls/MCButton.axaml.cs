using System;
using Antique_Tycoon.Services;
using Avalonia;
using Avalonia.Controls;
using LibVLCSharp.Shared;
using Microsoft.Extensions.DependencyInjection;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Views.Controls;

public partial class McButton : Button
{
  [GeneratedDirectProperty(true)]
  public partial bool IsPlaySound { get; set; }
  
  [GeneratedDirectProperty("Assets/SFX/MCButtonPressed.mp3")]
  public partial string MediaSource { get; set; }
  
  private void PlayClickSound()
  {
    // 【关键点】设计模式检查
    // 如果是在 Visual Studio 的设计预览器里，App.Current 可能为空，
    // 或者 Services 没配好。为了防止预览器崩溃，加个判断。
    if (Design.IsDesignMode) return;

    try
    {
      // 【核心】服务定位：主动去找 SoundService
      // 使用 ? 操作符防止 App 或 Services 为空导致崩溃
      var soundService = App.Current.Services.GetService<SoundService>();

      // 如果找到了服务，就播放
      soundService?.PlaySoundEffect(MediaSource);
    }
    catch
    {
      // 忽略音效错误，不要因为它影响按钮功能
    }
  }

  protected override void OnClick()
  {
    base.OnClick();

    // 2. 只有开启了声音才播放
    if (IsPlaySound)
    {
      PlayClickSound();
    }
  }
}