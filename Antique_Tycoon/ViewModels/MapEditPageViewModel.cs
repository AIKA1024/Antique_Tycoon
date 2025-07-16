using System.Threading.Tasks;
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
  private async Task CreateEntity(string type)
  {
    await Task.Delay(10);//命令触发比menuitem的tap时间早，导致PointerPosition没更新，这样等有点💩，但目前没问题
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