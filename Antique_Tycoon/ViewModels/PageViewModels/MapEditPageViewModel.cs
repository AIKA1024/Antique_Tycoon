using System.Threading.Tasks;
using Antique_Tycoon.Models.Cell;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Threading;
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
  private void CreateEntity(string type)
  {
    Dispatcher.UIThread.Post(() =>
    {
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
    }, DispatcherPriority.Render);
  }

  [RelayCommand]
  private void RemoveEntity(CanvasEntity target)
  {
    MapEntities.Remove(target);
  }
}