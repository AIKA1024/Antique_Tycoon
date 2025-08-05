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
    var canvasSize = _canvas.Bounds.Size;
    var containerSize = EntityListBox.Bounds.Size;

    if (canvasSize.Width <= 0 || canvasSize.Height <= 0)
      return;

    // 缩放因子
    double scaleX = containerSize.Width / canvasSize.Width;
    double scaleY = containerSize.Height / canvasSize.Height;
    double scale = Math.Min(scaleX, scaleY);

    // 更新 ViewModel 中的缩放值
    _mapEditPageViewModel.Scale = scale;

    // 正确的偏移量计算
    double offsetX = (containerSize.Width - canvasSize.Width * scale) / 2.0;
    double offsetY = (containerSize.Height - canvasSize.Height * scale) / 2.0;

    // 更新 ViewModel 中的偏移值
    _mapEditPageViewModel.Offset = new Point(offsetX, offsetY);
  }
  
  public Bitmap RenderCanvasToBitmap()
  {
    // 获取控件尺寸
    _canvas.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
    _canvas.Arrange(new Rect(_canvas.DesiredSize));

    var width = (int)(_canvas.Bounds.Width * 1);
    var height = (int)(_canvas.Bounds.Height * 1);

    var rtb = new RenderTargetBitmap(new PixelSize(width, height), new Vector(96, 96));

    // 使用 DrawingContext 绘制 Canvas
    using var ctx = rtb.CreateDrawingContext(false);
    // 应用缩放变换
    using (ctx.PushTransform(new ScaleTransform(1, 1).Value))
    {
      ctx.DrawRectangle(new VisualBrush(_canvas), null, new Rect(0, 0, _canvas.Bounds.Width, _canvas.Bounds.Height));
    }
    return rtb;
  }
}