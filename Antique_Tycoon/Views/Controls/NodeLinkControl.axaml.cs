using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Connections;
using Antique_Tycoon.Models.Node;
using Antique_Tycoon.ViewModels;
using Antique_Tycoon.Views.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Microsoft.Extensions.DependencyInjection;
using PropertyGenerator.Avalonia;
using System;
using System.Collections.Generic;
using System.Linq;
using Antique_Tycoon.Behaviors;

namespace Antique_Tycoon.Views.Controls;

public partial class NodeLinkControl : ContentControl
{
  [GeneratedDirectProperty] public partial NodeModel NodeModel { get; set; }
  [GeneratedDirectProperty] public partial Map Map { get; set; }
  [GeneratedDirectProperty] public partial Panel LineCanvas { get; set; }
}