using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Antique_Tycoon.Views.Controls;

public class XListBoxItem : ListBoxItem
{
  private bool _leftPressed;

  protected override void OnPointerPressed(PointerPressedEventArgs e)
  {
    if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed ||
        e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
    {
      e.Handled = true; // 阻止右键选中、左键按下就选中
      _leftPressed = true;
      return;
    }

    base.OnPointerPressed(e);
  }

  protected override void OnPointerReleased(PointerReleasedEventArgs e)
  {
    if (_leftPressed && e.InitialPressMouseButton == MouseButton.Left)
    {
      var listBox = this.FindAncestorOfType<ListBox>();
      if (listBox != null)
      {
        var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
        var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
        var item = this.DataContext;

        if (listBox.SelectionMode == SelectionMode.Multiple || listBox.SelectionMode == SelectionMode.Toggle)
        {
          if (ctrl)
          {
            if (listBox.SelectedItems.Contains(item))
              listBox.SelectedItems.Remove(item);
            else
              listBox.SelectedItems.Add(item);
          }
          else if (shift)
          {
            int startIndex = 0;
            if (listBox.SelectedItems.Count > 0)
              startIndex = listBox.Items.IndexOf(listBox.SelectedItems[0]);
            int endIndex = listBox.Items.IndexOf(item);

            int min = Math.Min(startIndex, endIndex);
            int max = Math.Max(startIndex, endIndex);

            listBox.SelectedItems.Clear();
            for (int i = min; i <= max; i++)
            {
              listBox.SelectedItems.Add(listBox.Items[i]);
            }
          }
          else
          {
            listBox.SelectedItems.Clear();
            listBox.SelectedItems.Add(item);
          }
        }
        else
        {
          listBox.SelectedItem = item;
        }
      }

      _leftPressed = false;
      e.Handled = true;
    }
    else
    {
      base.OnPointerReleased(e);
    }
  }
}