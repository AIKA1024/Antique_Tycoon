using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Nodes;

public partial class Mine:NodeModel
{
  [ObservableProperty]
  public partial decimal Charge { get; set; } = 200;
  
  public override string? Tooltip
  {
    get => !string.IsNullOrEmpty(field) ? field : "有机会获得古玩";
    set;
  }
}