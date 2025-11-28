using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Antique_Tycoon.Extensions;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Node;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Antique_Tycoon.ViewModels.PageViewModels;
using Antique_Tycoon.Views.Controls;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
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
  [GeneratedDirectProperty] public partial bool IsEditing { get; set; }
  [GeneratedDirectProperty] public partial Map CurrentMap { get; set; }
  public Point PointerPosition { get; set; }

  [GeneratedDirectProperty] public partial CanvasItemModel SelectedMapEntity { get; set; }
  [GeneratedDirectProperty] public partial ObservableCollection<CanvasItemModel> SelectedMapEntities { get; set; } = [];

  public GameCanvas()
  {
    InitializeComponent();
    AddHandler(Connector.ConnectedEvent, OnConnectorConnected, RoutingStrategies.Bubble);
    AddHandler(Connector.CancelConnectEvent, OnConnectorCancelConnect, RoutingStrategies.Bubble);
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
      ctx.DrawRectangle(new VisualBrush(_canvas), null, new Rect(0, 0, _canvas.Bounds.Width, _canvas.Bounds.Height));
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
              Background = CurrentMap.NodeDefaultBackground
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
            Background = CurrentMap.NodeDefaultBackground
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