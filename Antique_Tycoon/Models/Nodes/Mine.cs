using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Nodes;

public partial class Mine:NodeModel
{
  [ObservableProperty]
  public partial int Charge { get; set; } = 200;
}