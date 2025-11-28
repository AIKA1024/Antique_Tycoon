using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Antique_Tycoon.Views.Controls;

public class XListBoxItem : ListBoxItem
{
  private bool _leftPressed;
  private bool _dragging;
  private Point _pressPoint;
  public double DragStartThreshold { get; set; } = 2;

  protected override void OnPointerPressed(PointerPressedEventArgs e)
  {
    if (e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
      _leftPressed = true;

    e.Handled = true; // 阻止右键选中、左键按下就选中
    _pressPoint = e.GetPosition(this);
    _dragging = false;
  }

  protected override void OnPointerReleased(PointerReleasedEventArgs e)
  {
    if (_leftPressed)
    {
      var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
      if (!_dragging) // 点击而非拖动
      {
        RaiseEvent(new TappedEventArgs(TappedEvent, e));
        if (!ctrl)
          SelectItem(e);
        else
        {
          var xListBox = this.FindAncestorOfType<XListBox>();
          int index = xListBox?.SelectedItems.IndexOf(DataContext) ?? -1;
          if (index > -1)
            xListBox?.SelectedItems?.RemoveAt(index);
          else
            xListBox?.SelectedItems?.Add(DataContext);
        }
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

        var xListBox = this.FindAncestorOfType<XListBox>();
        if (xListBox != null &&
            !(e.KeyModifiers.HasFlag(KeyModifiers.Control) || e.KeyModifiers.HasFlag(KeyModifiers.Shift)))
        {
          var item = DataContext;
          if (!xListBox.SelectedItems.Contains(item))
          {
            // listBox.SelectedItems.Clear();
            xListBox.SelectedItem = item; // 拖拽开始时先选中当前项
          }
        }

        SelectItem(e);
      }
    }

    base.OnPointerMoved(e);
  }

  private void SelectItem(PointerEventArgs e)
  {
    var xListBox = this.FindAncestorOfType<XListBox>();
    if (xListBox == null) return;

    var ctrl = e.KeyModifiers.HasFlag(KeyModifiers.Control);
    var shift = e.KeyModifiers.HasFlag(KeyModifiers.Shift);
    var item = DataContext;

    if (xListBox.SelectionMode == SelectionMode.Multiple || xListBox.SelectionMode == SelectionMode.Toggle)
    {
      if (ctrl)
      {
        if (!xListBox.SelectedItems.Contains(item))
          xListBox.SelectedItems.Add(item);
      }
      else if (shift)
      {
        int startIndex = 0;
        if (xListBox.SelectedItems.Count > 0)
          startIndex = xListBox.Items.IndexOf(xListBox.SelectedItems[0]);
        int endIndex = xListBox.Items.IndexOf(item);

        int min = Math.Min(startIndex, endIndex);
        int max = Math.Max(startIndex, endIndex);

        xListBox.SelectedItems.Clear();
        for (int i = min; i <= max; i++)
        {
          xListBox.SelectedItems.Add(xListBox.Items[i]);
        }
      }
      else if (!_dragging || xListBox.SelectedItems?.Count == 0)
      {
        xListBox.SelectedItem = item;
      }
    }
    else
    {
      xListBox.SelectedItem = item;
    }
  }
}