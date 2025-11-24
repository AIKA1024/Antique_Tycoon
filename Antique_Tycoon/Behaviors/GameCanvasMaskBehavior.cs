using System;
using System.Diagnostics;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Views.Controls;
using Antique_Tycoon.Views.Widgets;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Behaviors;

public class GameCanvasMaskBehavior : Behavior<GameCanvas>
{
    private Canvas _targetCanvas;

    protected override void OnAttached()
    {
        base.OnAttached();
        Dispatcher.UIThread.Post(() =>
        {
            var listBox = AssociatedObject.FindLogicalDescendantOfType<XListBox>();
            var itemsPresenter = listBox.FindVisualChild<ItemsPresenter>();
            _targetCanvas = itemsPresenter.FindLogicalDescendantOfType<Canvas>();
            
            var maskBorder = new Border
            {
                Background = Brushes.Black,
                Opacity = 0.6,
                ZIndex = 1,
                Width = _targetCanvas.Bounds.Width,
                Height = _targetCanvas.Bounds.Height
            };
            Canvas.SetLeft(maskBorder, 0);
            Canvas.SetTop(maskBorder, 0);
            _targetCanvas.Children.Add(maskBorder);
        });
    }
}