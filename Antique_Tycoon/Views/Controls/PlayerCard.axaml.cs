using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Markup.Xaml;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Views.Controls;

[PseudoClasses(":turn")]
public partial class PlayerCard : UserControl
{
  public static readonly StyledProperty<bool> IsMyTurnProperty =
    AvaloniaProperty.Register<PlayerCard, bool>(nameof(IsMyTurn));

  public bool IsMyTurn
  {
    get => GetValue(IsMyTurnProperty);
    set
    {
      SetValue(IsMyTurnProperty, value);
      PseudoClasses.Set(":turn", value);
    }
  }

  public PlayerCard()
  {
    InitializeComponent();
  }
}