using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Nodes;

public partial class TeleportationPoint : NodeModel
{
    [ObservableProperty] public partial decimal Charge { get; set; } = 200;

    [ObservableProperty] public partial decimal Value { get; set; }
}