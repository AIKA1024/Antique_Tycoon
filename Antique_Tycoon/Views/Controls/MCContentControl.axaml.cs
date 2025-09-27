using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Antique_Tycoon.Views.Controls;

public class McContentControl : ContentControl
{
  public static readonly StyledProperty<string> HeaderProperty =
    AvaloniaProperty.Register<McContentControl, string>(nameof(Header),"");

  public string Header
  {
    get => GetValue(HeaderProperty);
    set => SetValue(HeaderProperty, value);
  }
}