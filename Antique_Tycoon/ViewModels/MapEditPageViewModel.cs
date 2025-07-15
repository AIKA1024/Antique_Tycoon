using Antique_Tycoon.Models.Cell;
using Avalonia;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Antique_Tycoon.ViewModels;

public partial class MapEditPageViewModel:DragAndZoomViewModel
{
  public AvaloniaList<CanvasEntity> MapEntities { get; set; } = [];
  public Point PointerPosition { get; set; }

  [RelayCommand]
  private void CreateEntity(string type)
  {
    switch (type)
    {
      case "玩家出生点":
        MapEntities.Add(new SpawnPoint{Left = PointerPosition.X,Top = PointerPosition.Y,Title = "玩家出生点"});
        break;
      case "地产":
        break;
      case "自定义事件":
        break;
    }
  }
}