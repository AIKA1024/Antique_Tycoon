using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace Antique_Tycoon.Views.Controls;

public class MCTitleBar : ContentControl
{
  public static readonly StyledProperty<string> TitleProperty =
    AvaloniaProperty.Register<MCTitleBar, string>(nameof(Title),"");

  public string Title
  {
    get => GetValue(TitleProperty);
    set => SetValue(TitleProperty, value);
  }
}