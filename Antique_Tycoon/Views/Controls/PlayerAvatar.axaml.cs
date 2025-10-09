using Avalonia.Controls.Primitives;
using LibVLCSharp.Shared;
using PropertyGenerator.Avalonia;
using Player = Antique_Tycoon.Models.Player;

namespace Antique_Tycoon.Views.Controls;

public partial class PlayerAvatar : TemplatedControl
{
  [GeneratedDirectProperty]
  public partial Player Player { get; set; }
  [GeneratedDirectProperty]
  public Media BuyLowValueItemSound { get; }
  [GeneratedDirectProperty]
  public Media PlayerMoveSound { get; }
}