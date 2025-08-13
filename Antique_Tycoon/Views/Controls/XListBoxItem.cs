using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;

namespace Antique_Tycoon.Views.Controls;

public class XListBoxItem : ListBoxItem
{
  private bool _leftPressed;
  private bool _dragging;
  private Point _pressPoint;
  public double DragStartThreshold { get; set; } = 4;

  protected override void OnPointerPressed(PointerPressedEventArgs e)
  {
    if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed ||
        e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.RightButtonPressed)
    {
      e.Handled = true; // 阻止右键选中、左键按下就选中
      _leftPressed = true;
      _pressPoint = e.GetPosition(this);
      _dragging = false;
      return;
    }

    base.OnPointerPressed(e);
  }

  protected override void OnPointerReleased(PointerReleasedEventArgs e)
  {
    if (_leftPressed)
    {
      if (!_dragging) // 点击而非拖动
      {
        SelectItem(e);
      }

      _leftPressed = false;
      _dragging = false;
      e.Handled = true;
    }
    else
    {
      base.OnPointerReleased(e);
    }
  }

  protected override void OnPointerMoved(PointerEventArgs e)
  {
    if (_leftPressed && !_dragging)
    {
      var pos = e.GetPosition(this);
      if (Math.Abs(pos.X - _pressPoint.X) > DragStartThreshold ||
          Math.Abs(pos.Y - _pressPoint.Y) > DragStartThreshold)
      {
        _dragging = true;

        var listBox = this.FindAncestorOfType<ListBox>();
        if (listBox != null && 
            !(e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Shift)))
        {
          var item = DataContext;
          if (!listBox.SelectedItems.Contains(item))
          {
            // listBox.SelectedItems.Clear();
            listBox.SelectedItem = item; // 拖拽开始时先选中当前项
          }
        }

        SelectItem(e);
      }
    }

    base.OnPointerMoved(e);
  }

  private void SelectItem(PointerEventArgs e)
  {
    var listBox = this.FindAncestorOfType<ListBox>();
    if (listBox == null) return;

    var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
    var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
    var item = DataContext;

    if (listBox.SelectionMode == SelectionMode.Multiple || listBox.SelectionMode == SelectionMode.Toggle)
    {
      if (ctrl)
      {
        if (!listBox.SelectedItems.Contains(item))
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
      else if (!_dragging || listBox.SelectedItems?.Count == 0)
      {
        listBox.SelectedItem = item;
      }
    }
    else
    {
      listBox.SelectedItem = item;
    }
  }
}