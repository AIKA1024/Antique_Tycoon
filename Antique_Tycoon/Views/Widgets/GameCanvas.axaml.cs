using System;
using System.Linq;
using Antique_Tycoon.ViewModels.PageViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.VisualTree;

namespace Antique_Tycoon.Views.Widgets;

public partial class GameCanvas : UserControl
{
  private Canvas _canvas;
  private MapEditPageViewModel _mapEditPageViewModel;

  public GameCanvas()
  {
    InitializeComponent();
  }

  protected override void OnLoaded(RoutedEventArgs e)
  {
    base.OnLoaded(e);
    _canvas = EntityListBox.GetVisualDescendants()
      .OfType<Canvas>()
      .FirstOrDefault()!;
    Focus();
    _mapEditPageViewModel = (MapEditPageViewModel)DataContext;
    _mapEditPageViewModel.RequestRenderControl = RenderCanvasToBitmap;
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

    var canvasWidth = vm.Map.CanvasWidth;
    var canvasHeight = vm.Map.CanvasHeight;

    var availableWidth = parent.Bounds.Width;
    var availableHeight = parent.Bounds.Height;

    // 计算适合的缩放比例（保留比例）
    var scaleX = availableWidth / canvasWidth;
    var scaleY = availableHeight / canvasHeight;
    var scale = Math.Min(scaleX, scaleY);

    // 更新 ViewModel 中的 Scale
    vm.Scale = scale;

    // 计算居中偏移
    var offsetX = (availableWidth - canvasWidth * scale) / 2;
    var offsetY = (availableHeight - canvasHeight * scale) / 2;

    vm.Offset = new Point(offsetX, offsetY);
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
}