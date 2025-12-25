using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Antique_Tycoon.Behaviors;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Node;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Antique_Tycoon.ViewModels.PageViewModels;
using Antique_Tycoon.Views.Controls;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Views.Widgets;

public partial class GameCanvas : UserControl
{
    private Canvas _canvas;
    private MapEditPageViewModel _mapEditPageViewModel;
    private GameManager _gameManager = App.Current.Services.GetRequiredService<GameManager>();
    [GeneratedDirectProperty] public partial bool IsEditing { get; set; }
    [GeneratedDirectProperty] public partial Map CurrentMap { get; set; }
    public Point PointerPosition { get; set; }

    [GeneratedDirectProperty] public partial CanvasItemModel SelectedMapEntity { get; set; }

    [GeneratedDirectProperty]
    public partial ObservableCollection<CanvasItemModel> SelectedMapEntities { get; set; } = [];

    public GameCanvas()
    {
        InitializeComponent();
        AddHandler(Connector.ConnectedEvent, OnConnectorConnected, RoutingStrategies.Bubble);
        AddHandler(Connector.CancelConnectEvent, OnConnectorCancelConnect, RoutingStrategies.Bubble);
        WeakReferenceMessenger.Default.Register<StartPlayerMoveAnimation>(this, ReceivePlayerMoveAnimationMessage);
    }


    private async Task StartPlayerMoveAnimationAsync(StartPlayerMoveAnimation message)
    {
        var nodes = message.Path
            .Select(s => (NodeModel)_gameManager.SelectedMap.EntitiesDict[s])
            .ToArray();

        if (nodes.Length < 1) return;

        // 建立 Node 与 Control 的映射，方便获取 Bounds
        var nodeControlsDic = new Dictionary<NodeModel, NodeLinkControl>();
        foreach (var node in nodes)
        foreach (var canvasItem in _canvas.Children)
            if (canvasItem is NodeLinkControl nodeControl && node == nodeControl.DataContext as NodeModel)
            {
                nodeControlsDic.Add(node, nodeControl);
                break;
            }

        const double imgWidth = 24;
        const double imgHeight = 24; // 假设高度也是 32
        var image = new Image { Source = message.Player.Avatar, Width = imgWidth, Height = imgHeight };

        // --- 核心逻辑：计算居中位置的辅助方法 ---
        Point GetCenterPosition(NodeModel node)
        {
            if (nodeControlsDic.TryGetValue(node, out var control))
            {
                // 使用 Control 的 Bounds 来获取精确的中心点
                // Bounds.Center 是相对于 Canvas 的中心位置
                var center = control.Bounds.Center;
                return new Point(center.X - (imgWidth / 2.0), center.Y - (imgHeight / 2.0));
            }

            // 回退方案：如果没找到 Control，使用 NodeModel 的基础坐标
            return new Point(
                node.Left + (node.Width / 2.0) - (imgWidth / 2.0),
                node.Top + (node.Height / 2.0) - (imgHeight / 2.0));
        }

        // 1. 设置初始位置 (Nodes[0] 的中心)
        var startPos = GetCenterPosition(nodes[0]);
        Canvas.SetLeft(image, startPos.X);
        Canvas.SetTop(image, startPos.Y);

        var transform = new TranslateTransform();
        image.RenderTransform = transform;
        _canvas.Children.Add(image);

        double currentOffsetX = 0;
        double currentOffsetY = 0;

        foreach (var node in nodes[1..])
        {
            var targetPos = GetCenterPosition(node);
            double targetX = targetPos.X - startPos.X;
            double targetY = targetPos.Y - startPos.Y;

            var animation = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(500),
                Easing = new CubicEaseInOut(),
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0.0),
                        Setters = {
                            new Setter(TranslateTransform.XProperty, currentOffsetX),
                            new Setter(TranslateTransform.YProperty, currentOffsetY)
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1.0),
                        Setters = {
                            new Setter(TranslateTransform.XProperty, targetX),
                            new Setter(TranslateTransform.YProperty, targetY)
                        }
                    }
                }
            };

            // 关键改动 2: 在运行新动画之前，强制同步一次本地值，确保动画起点和当前位置物理对齐
            transform.X = currentOffsetX;
            transform.Y = currentOffsetY;

            // 运行动画
            await animation.RunAsync(image);

            // 关键改动 3: 动画结束后，立即更新 currentOffset 供下一轮使用
            currentOffsetX = targetX;
            currentOffsetY = targetY;
    
            // 关键改动 4: 再次强制同步本地值
            transform.X = targetX;
            transform.Y = targetY;

        }

        _canvas.Children.Remove(image);
    }

    private void ReceivePlayerMoveAnimationMessage(object recipient, StartPlayerMoveAnimation message)
    {
        message.Reply(StartPlayerMoveAnimationAsync(message));
    }

    private void OnConnectorConnected(object? sender, Connector.ConnectedRoutedEventArgs e)
    {
        _mapEditPageViewModel.CurrentMap.Entities.Add(e.Connection);
    }

    private void OnConnectorCancelConnect(object? sender, Connector.CancelConnectRoutedEventArgs e)
    {
        if (e.Source is Connector connector)
        {
            connector.CancelConnects(_mapEditPageViewModel.CurrentMap);
            e.Handled = true;
        }
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _canvas = EntityListBox.GetVisualDescendants()
            .OfType<Canvas>()
            .FirstOrDefault()!;
        Focus();

        if (DataContext is MapEditPageViewModel mapEditPageViewModel)
        {
            _mapEditPageViewModel = mapEditPageViewModel;
            _mapEditPageViewModel.RequestRenderControl = RenderCanvasToBitmap;
        }
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);

        if (e.Key == Key.Space && e.KeyModifiers.HasFlag(KeyModifiers.Control))
            // ZoomToFit();
            FitCanvasToContainer();
    }

    void FitCanvasToContainer()
    {
        if (EntityListBox == null || DataContext is not MapEditPageViewModel vm)
            return;

        if (EntityListBox.GetVisualParent() is not Control parent)
            return;

        var canvasWidth = vm.CurrentMap.CanvasWidth;
        var canvasHeight = vm.CurrentMap.CanvasHeight;

        var availableWidth = parent.Bounds.Width;
        var availableHeight = parent.Bounds.Height;

        // 计算适合的缩放比例（保留比例）
        var scaleX = availableWidth / canvasWidth;
        var scaleY = availableHeight / canvasHeight;
        var scale = Math.Min(scaleX, scaleY);

        // 更新 ViewModel 中的 Scale
        _mapEditPageViewModel.CurrentMap.Scale = scale;

        // 计算居中偏移
        var offsetX = (availableWidth - canvasWidth * scale) / 2;
        var offsetY = (availableHeight - canvasHeight * scale) / 2;

        _mapEditPageViewModel.CurrentMap.Offset = new Point(offsetX, offsetY);
    }

    public Bitmap RenderCanvasToBitmap()
    {
        // 获取控件尺寸
        const double scale = 0.1;
        _canvas.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        _canvas.Arrange(new Rect(_canvas.DesiredSize));

        var width = (int)(_canvas.Bounds.Width * scale);
        var height = (int)(_canvas.Bounds.Height * scale);

        var rtb = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));

        // 使用 DrawingContext 绘制 Canvas
        using var ctx = rtb.CreateDrawingContext(false);
        // 应用缩放变换
        using (ctx.PushTransform(new ScaleTransform(scale, scale).Value))
        {
            ctx.DrawRectangle(new VisualBrush(_canvas), null,
                new Rect(0, 0, _canvas.Bounds.Width, _canvas.Bounds.Height));
        }

        return rtb;
    }

    private void InputElement_OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.InitialPressMouseButton == MouseButton.Left)
            EntityListBox.SelectedItems?.Clear();
    }

    [RelayCommand]
    private void CreateEntity(string type)
    {
        Dispatcher.UIThread.Post(async void () =>
        {
            switch (type)
            {
                case "玩家出生点":
                    if (string.IsNullOrEmpty(CurrentMap.SpawnNodeUuid))
                    {
                        var spawnPoint = new SpawnPoint
                        {
                            Left = PointerPosition.X,
                            Top = PointerPosition.Y,
                            Title = "玩家出生点",
                        };
                        CurrentMap.Entities.Add(spawnPoint);
                        CurrentMap.SpawnNodeUuid = spawnPoint.Uuid;
                    }
                    else
                        await App.Current.Services.GetRequiredService<DialogService>().ShowDialogAsync(
                            new MessageDialogViewModel
                            {
                                Title = "提示",
                                Message = "只能有一个出生点"
                            });

                    break;
                case "地产":
                    CurrentMap.Entities.Add(new Estate
                    {
                        Left = PointerPosition.X,
                        Top = PointerPosition.Y,
                        Title = "某生态群系",
                    });
                    break;
                case "自定义事件":
                    break;
            }
        }, DispatcherPriority.Render);
    }

    [RelayCommand]
    private void RemoveEntity(NodeModel target)
    {
        if (!IsEditing)
            return;
        foreach (var model in target.ConnectorModels)
            model.CancelConnects(CurrentMap);
        if (target.Uuid == CurrentMap.SpawnNodeUuid)
            CurrentMap.SpawnNodeUuid = "";
        CurrentMap.Entities.Remove(target);
    }

    [RelayCommand]
    private void NodeClicked(string uuid)
    {
        WeakReferenceMessenger.Default.Send(
            new NodeClickedMessage(uuid));
    }
}