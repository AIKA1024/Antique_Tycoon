using Avalonia.Controls;
using Avalonia.Input;

namespace Antique_Tycoon.Views.Controls;

public class XListbox : ListBox
{
  protected override Control CreateContainerForItemOverride(object? item, int index, object? recycleKey)
  {
    return new XListBoxItem();
  }
}