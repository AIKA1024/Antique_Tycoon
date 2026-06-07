using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Views.Controls;

public partial class ToggleLever : TemplatedControl
{
  [GeneratedDirectProperty(true)]
  public partial bool IsPlaySound { get; set; }
}