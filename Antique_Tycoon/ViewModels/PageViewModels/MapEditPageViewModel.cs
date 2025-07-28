using System.Threading.Tasks;
using Antique_Tycoon.Models.Cell;
using Avalonia;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class MapEditPageViewModel:DragAndZoomViewModel
{
  public AvaloniaList<CanvasEntity> MapEntities { get; set; } = [];
  public AvaloniaList<CanvasEntity> SelectedMapEntities { get;} = [];
  
  [ObservableProperty]
  private CanvasEntity? _selectedMapEntity;
  
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
        MapEntities.Add(new Estate{Left = PointerPosition.X,Top = PointerPosition.Y,Title = "某生态群系"});
        break;
      case "自定义事件":
        break;
    }
  }

  [RelayCommand]
  private void RemoveEntity(CanvasEntity target)
  {
    MapEntities.Remove(target);
  }
}