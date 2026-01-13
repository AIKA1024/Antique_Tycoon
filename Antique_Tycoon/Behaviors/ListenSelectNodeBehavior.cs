using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
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

public partial class ListenSelectNodeBehavior : Behavior<GameCanvas>
{
    private Canvas _targetCanvas;
    private Border _targetMaskBorder;
    private TaskCompletionSource<string>? _nodeClickTcs;
    

    protected override void OnAttached()
    {
        base.OnAttached();
        WeakReferenceMessenger.Default.Register<GameMaskShowMessage>(this,ReceiveGameMaskShowMessage);
        WeakReferenceMessenger.Default.Register<NodeClickedMessage>(this, ReceiveNodeClicked);
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

    private async void ReceiveGameMaskShowMessage(object recipient, GameMaskShowMessage message)
    {
        int oldZIndex = message.SelectableNodes[0].ZIndex;
        _targetMaskBorder.IsVisible = true;
        foreach (var item in message.SelectableNodes)
            item.ZIndex = 4;
        // 2. 【核心修改】直接把 Task 扔给 Reply，不要在这里 await
        var clickTask = AwaitNodeClickAsync();
        message.Reply(clickTask);

        // 3. 处理后续的清理工作（遮罩隐藏等）
        // 由于我们不能在当前方法 await（会导致报错），
        // 我们需要在这个 Task 完成后异步执行清理
        await clickTask.ContinueWith(t =>
        {
            // 必须切回 UI 线程操作
            Dispatcher.UIThread.Post(() =>
            {
                foreach (var item in message.SelectableNodes)
                    item.ZIndex = oldZIndex;
                _targetMaskBorder.IsVisible = false;
            });
        }, TaskScheduler.Current);
    }
    
    private void ReceiveNodeClicked(object sender, NodeClickedMessage message)
    {
        // 过滤无效点击：仅处理「高亮模式下」的点击
        if (!_targetMaskBorder.IsVisible) return;

        // 触发 TaskCompletionSource 完成，返回结果
        _nodeClickTcs?.SetResult(message.NodeUuid);
        _nodeClickTcs = null; // 重置，避免重复触发
    }
    private async Task<string> AwaitNodeClickAsync()
    {
        // 2. 初始化 TaskCompletionSource，用于阻塞等待
        _nodeClickTcs = new TaskCompletionSource<string>();

        // 3. 等待点击结果（非 UI 阻塞，可 await）
        var selectedCardUuid = await _nodeClickTcs.Task;

        // 5. 返回选中的卡片 UUID
        return selectedCardUuid;
    }
}