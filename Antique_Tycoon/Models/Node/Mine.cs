using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Node;

public partial class Mine:NodeModel
{
  [ObservableProperty]
  public partial int Charge { get; set; } = 200;
}