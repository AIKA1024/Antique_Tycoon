using Avalonia.Controls;
using Avalonia.Input;

namespace Antique_Tycoon.Views.Controls;

public class XListBoxItem: ListBoxItem
{
  protected override void OnPointerPressed(PointerPressedEventArgs e)
  {
    if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
    {
      e.Handled = true; // 阻止右键选中
      return;
    }
    base.OnPointerPressed(e);
  }
}