using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Antique_Tycoon.Views.Controls;

public class MCContentControl : ContentControl
{
  public static readonly StyledProperty<string> HeaderProperty =
    AvaloniaProperty.Register<MCContentControl, string>(nameof(Header),"");

  public string Header
  {
    get => GetValue(HeaderProperty);
    set => SetValue(HeaderProperty, value);
  }
}