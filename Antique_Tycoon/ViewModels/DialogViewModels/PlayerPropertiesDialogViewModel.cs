using Antique_Tycoon.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.ViewModels.DialogViewModels;

public partial class PlayerPropertiesDialogViewModel(Player player):DialogViewModelBase
{
    [ObservableProperty] public partial Player Player { get; set; } = player;
    
}