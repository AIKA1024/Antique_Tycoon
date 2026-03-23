using Avalonia;
using Avalonia.Controls;

namespace Antique_Tycoon.Utilities;

public class NodeProps:AvaloniaObject
{
  public static readonly AttachedProperty<bool> IsHeightLightProperty =
    AvaloniaProperty.RegisterAttached<NodeProps, Control, bool>("IsHeightLight");

  public static void SetIsHeightLight(Control obj, bool value) => obj.SetValue(IsHeightLightProperty, value);
  public static bool GetIsHeightLight(Control obj) => obj.GetValue(IsHeightLightProperty);
  
  static NodeProps()
  {
    // IsHeightLightProperty.Changed.AddClassHandler<Control>(IsHeightLightChanged);
  }

  private static void IsHeightLightChanged(Control sender, AvaloniaPropertyChangedEventArgs e)
  {
    
  }
}