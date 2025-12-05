using System;
using System.Diagnostics;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Views.Controls;
using Antique_Tycoon.Views.Widgets;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;
using CommunityToolkit.Mvvm.Messaging;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Behaviors;

public class GameCanvasMaskBehavior : Behavior<GameCanvas>
{
    private Canvas _targetCanvas;
    private Border _targetMaskBorder;

    protected override void OnAttached()
    {
        base.OnAttached();
        WeakReferenceMessenger.Default.Register<GameMaskShowMessage>(this,ReceiveGameMaskShowMessage);
        Dispatcher.UIThread.Post(() =>
        {
            var listBox = AssociatedObject.FindLogicalDescendantOfType<XListBox>();
            var itemsPresenter = listBox.FindVisualChild<ItemsPresenter>();
            _targetCanvas = itemsPresenter.FindLogicalDescendantOfType<Canvas>();
            
            _targetMaskBorder = new Border
            {
                Background = Brushes.Black,
                Opacity = 0.6,
                ZIndex = 3,
                Width = _targetCanvas.Bounds.Width,
                Height = _targetCanvas.Bounds.Height,
                IsVisible = false
            };
            Canvas.SetLeft(_targetMaskBorder, 0);
            Canvas.SetTop(_targetMaskBorder, 0);
            _targetCanvas.Children.Add(_targetMaskBorder);
        });
    }

    private void ReceiveGameMaskShowMessage(object recipient, GameMaskShowMessage message)
    {
        _targetMaskBorder.IsVisible = message.IsShowMask;
    }
}