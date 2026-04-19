using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Udp;
using Avalonia.Layout;
using CommunityToolkit.Mvvm.Input;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public partial class AddServiceDialogViewModel : DialogViewModelBase<ServiceInfo?>
{
  public ServiceInfo ServiceInfo { get; set; } = new();

  public AddServiceDialogViewModel()
  {
    HorizontalAlignment = HorizontalAlignment.Stretch;
    MaxWidthPercent = .6f;
  }

  [RelayCommand]
  private void Confirm()
  {
    if (IsValidHostOrIp(ServiceInfo.Address))
    {
      CloseDialog(ServiceInfo);
    }
  }

  [RelayCommand]
  private void Cancel()
  {
    CloseDialog(null);
  }

  private static bool IsValidHostOrIp(string input)
  {
    if (string.IsNullOrWhiteSpace(input)) return false;

    // 1. 检测是否为严格的 IPv4 (1.1.1.1 这种)
    // 必须能解析成功，且包含 3 个点
    if (IPAddress.TryParse(input, out var ip))
    {
      if (ip.AddressFamily == AddressFamily.InterNetwork)
      {
        return input.Split('.').Length == 4;
      }

      // 如果你还需要支持 IPv6，可以在这里扩展
      return ip.AddressFamily == AddressFamily.InterNetworkV6;
    }

    // 2. 检测是否为合法的域名格式 (frp-ski.com 这种)
    // 域名不能包含协议头 (http://)，不能包含特殊字符
    // 使用 Uri.CheckHostName 配合一个简单的正则确保它至少有一个点
    if (Uri.CheckHostName(input) == UriHostNameType.Dns)
    {
      // 确保域名至少包含一个点（排除 localhost 这种纯单词）
      // 且不包含像 http:// 这种协议字符
      return input.Contains(".") && !input.Contains("://");
    }

    return false;
  }
}