using Antique_Tycoon.Views.Controls;
using Avalonia.Media;

namespace Antique_Tycoon.Models.Connections;

public class Connection
{
  public ConnectorModel Start { get; }
  public ConnectorModel End { get; }
  public ConnectionLine Line { get; }

  public Connection(ConnectorModel start, ConnectorModel end)
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