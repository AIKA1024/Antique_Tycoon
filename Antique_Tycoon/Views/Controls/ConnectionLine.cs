using System;
using Avalonia;
using Avalonia.Controls.Shapes;
using Avalonia.Media;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Views.Controls;

public partial class ConnectionLine : Path
{
  [GeneratedDirectProperty] public partial Point StartPoint { get; set; }
  [GeneratedDirectProperty] public partial Point EndPoint { get; set; }

  public double ArrowLength { get; set; } = 6;
  public double ShortenLength { get; set; } = 6;
  public double ArrowAngle { get; set; } = Math.PI / 6;
  

  public ConnectionLine()
  {
    ZIndex = 1;
  }
  
  //仅用于画零时线条
  public virtual void UpdateGeometry()
  {
    double maxCurviness = 0.3; // 最大弯曲度
    Vector delta = EndPoint - StartPoint;
  
    if (delta.Length < double.Epsilon)
    {
        // 起点终点重合，直接返回
        Data = null;
        return;
    }
  
    // 计算线段方向与水平轴的夹角（弧度）
    double angle = Math.Abs(Math.Atan2(delta.Y, delta.X)); // 0~pi
  
    // 转成 0~pi/2范围（因为弯曲对称）
    if (angle > Math.PI / 2)
        angle = Math.PI - angle;
  
    // 计算弯曲权重（0在0度和90度，1在45度）
    // 使用一个简单的三角函数平滑曲线：
    // 这里用 sin(2*angle)，因为sin(0)=0, sin(pi/2)=1, sin(pi)=0
    double weight = Math.Sin(2 * angle);
  
    // 计算实际弯曲度
    double curviness = maxCurviness * weight;
  
    // 计算法线向量
    Vector normal;
    if (Math.Abs(delta.X) < 1e-6 || Math.Abs(delta.Y) < 1e-6)
    {
        normal = new Vector(0, 0); // 严格水平或垂直时没有弯曲
    }
    else
    {
        normal = new Vector(-delta.Y, delta.X);
        normal = NormalizeToLength(normal, 1);
    }
  
    var midPoint = StartPoint + delta * 0.5;
    double length = delta.Length;
    Vector offset = normal * length * curviness;
  
    // 控制点倒“S”型偏移
    var p1 = midPoint - offset;
    var p2 = midPoint + offset;
  
    // 缩短起点和终点（你原来的逻辑）
    Vector startTangent = p1 - StartPoint;
    if (startTangent.Length < double.Epsilon)
        startTangent = new Vector(1, 0);
    startTangent = NormalizeToLength(startTangent, ShortenLength);
    var newStart = StartPoint + startTangent;
  
    Vector tangent = EndPoint - p2;
    if (tangent.Length < double.Epsilon)
        tangent = new Vector(0, 1);
    tangent = NormalizeToLength(tangent, ShortenLength);
    var newEnd = EndPoint - tangent;
  
    // 生成曲线
    var geometry = new PathGeometry
    {
        Figures =
        [
            new PathFigure
            {
                StartPoint = newStart,
                Segments =
                [
                    new BezierSegment
                    {
                        Point1 = p1,
                        Point2 = p2,
                        Point3 = newEnd
                    }
                ],
                IsClosed = false
            }
        ]
    };
  
    // 箭头
    Vector arrowDir = NormalizeToLength(p2 - newEnd, ArrowLength);
    var left = newEnd + RotateVector(arrowDir, ArrowAngle);
    var right = newEnd + RotateVector(arrowDir, -ArrowAngle);
  
    geometry.Figures.Add(new PathFigure
    {
        StartPoint = left,
        Segments =
        [
            new LineSegment { Point = newEnd },
            new LineSegment { Point = right }
        ],
        IsClosed = false
    });
  
    Data = geometry;
  }
  
  private static Vector NormalizeToLength(Vector v, double length)
  {
    if (v.Length < double.Epsilon)
      return new Vector(0, 0);
    return v * (length / v.Length);
  }
  
  private static Vector RotateVector(Vector v, double angle)
  {
    var cos = Math.Cos(angle);
    var sin = Math.Sin(angle);
    return new Vector(
      v.X * cos - v.Y * sin,
      v.X * sin + v.Y * cos
    );
  }
}