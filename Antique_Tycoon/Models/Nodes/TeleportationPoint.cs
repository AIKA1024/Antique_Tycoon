using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Nodes;

public partial class TeleportationPoint : NodeModel//就不给买了吧，麻烦又没人买
{
    [ObservableProperty] public partial decimal Charge { get; set; } = 200;
}