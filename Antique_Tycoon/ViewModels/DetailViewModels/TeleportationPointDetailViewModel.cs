using Antique_Tycoon.Models.Nodes;

namespace Antique_Tycoon.ViewModels.DetailViewModels;

public class TeleportationPointDetailViewModel(TeleportationPoint model): NodeDetailViewModel(model)
{
    public TeleportationPoint TeleportationPoint => (TeleportationPoint)Model;
}