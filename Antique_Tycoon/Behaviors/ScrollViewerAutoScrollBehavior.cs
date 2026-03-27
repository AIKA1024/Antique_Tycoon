using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.Xaml.Interactivity;

namespace Antique_Tycoon.Behaviors;

public class ScrollViewerAutoScrollBehavior: Behavior<ScrollViewer>
{
    private bool _wasAtBottom = true; 
    // 防止代码自动滚动时触发用户手动滚动的误判
    private bool _isAutoScrolling = false;

    protected override void OnAttached()
    {
        base.OnAttached();
        if (AssociatedObject != null)
        {
            // 监听 ScrollViewer 的各种属性变化
            AssociatedObject.PropertyChanged += OnScrollViewerPropertyChanged;
        }
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        if (AssociatedObject != null)
        {
            AssociatedObject.PropertyChanged -= OnScrollViewerPropertyChanged;
        }
    }

    private void OnScrollViewerPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (AssociatedObject == null) return;

        // 1. 如果是用户的滚动或可视区域大小发生了改变，重新评估是否在最底部
        if (e.Property == ScrollViewer.OffsetProperty || e.Property == ScrollViewer.ViewportProperty)
        {
            // 只有在非自动滚动的情况下，才更新状态（说明是用户手动滚动的）
            if (!_isAutoScrolling)
            {
                _wasAtBottom = CheckIsAtBottom(AssociatedObject);
            }
        }
        // 2. 如果是内容总高度发生了改变（说明里面添加了新内容）
        else if (e.Property == ScrollViewer.ExtentProperty)
        {
            // 如果内容增加前，用户是在最底部的，那就自动滚动到新底部
            if (_wasAtBottom)
            {
                ScrollToBottom();
            }
        }
    }

    /// <summary>
    /// 判断当前是否处于最底部
    /// </summary>
    private bool CheckIsAtBottom(ScrollViewer scrollViewer)
    {
        // 最大可滚动距离 = 内容总高度 - 当前可视窗口高度
        double maxScrollY = scrollViewer.Extent.Height - scrollViewer.Viewport.Height;
        
        // 如果内容还没填满一屏，默认算作在最底部
        if (maxScrollY <= 0) return true;

        // 加入 2.0 的容差值处理浮点数精度问题
        return scrollViewer.Offset.Y >= (maxScrollY - 2.0);
    }

    /// <summary>
    /// 执行滚动到底部
    /// </summary>
    private void ScrollToBottom()
    {
        if (AssociatedObject == null) return;

        _isAutoScrolling = true;

        // 延迟到 UI 布局更新后再滚动，确保能滚到最新的底部
        Dispatcher.UIThread.Post(() =>
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.ScrollToEnd();
            }
            // 滚动结束后解除标记
            _isAutoScrolling = false;
        }, DispatcherPriority.Background);
    }
}