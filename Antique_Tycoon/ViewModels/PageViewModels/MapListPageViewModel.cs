using Antique_Tycoon.Models;
using Avalonia.Collections;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class MapListPageViewModel:PageViewModelBase
{
  public AvaloniaList<Map> Maps { get; set; } = [];
}