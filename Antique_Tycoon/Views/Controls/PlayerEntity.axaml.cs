using Antique_Tycoon.Models;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using LibVLCSharp.Shared;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Views.Controls;

public partial class PlayerEntity : TemplatedControl
{
  [GeneratedDirectProperty]
  public partial Player Player { get; set; }
  [GeneratedDirectProperty]
  public Media BuyLowValueItemSound { get; }
  [GeneratedDirectProperty]
  public Media PlayerMoveSound { get; }
}