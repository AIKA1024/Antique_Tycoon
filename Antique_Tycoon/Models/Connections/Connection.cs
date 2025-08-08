using Antique_Tycoon.Views.Controls;
using Avalonia.Media;

namespace Antique_Tycoon.Models.Connections;

public class Connection
{
  public Connector Start { get; }
  public Connector End { get; }
  public ConnectionLine Line { get; }

  public Connection(Connector start, Connector end)
  {
    Start = start;
    End = end;

    Line = new ConnectionLine
    {
      Stroke = Brushes.White,
      StrokeThickness = 2
    };
    Update();
  }
  public void Update()
  {
    Line.StartPoint = Start.Anchor;
    Line.EndPoint = End.Anchor;
    Line.UpdateGeometry();
  }
}