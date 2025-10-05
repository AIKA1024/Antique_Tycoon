using System.Collections.ObjectModel;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Node;
using Avalonia.Controls;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Views.Controls;

public partial class NodeLinkControl : ContentControl
{
  [GeneratedDirectProperty] public partial NodeModel NodeModel { get; set; }
  [GeneratedDirectProperty] public partial Map Map { get; set; }
  [GeneratedDirectProperty] public partial Panel LineCanvas { get; set; }
}