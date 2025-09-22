using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Net;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class CreateRoomPageViewModel : PageViewModelBase
{
  private readonly CancellationTokenSource _cts = new();
  private readonly GameManager _gameManager = App.Current.Services.GetRequiredService<GameManager>();
  private readonly DialogService _dialogService = App.Current.Services.GetRequiredService<DialogService>();
  private readonly MapFileService _mapFileService = App.Current.Services.GetRequiredService<MapFileService>();

  [ObservableProperty] private string _roomName = "";
  [ObservableProperty]
  private Map? _selectedMap;
  
  public CreateRoomPageViewModel()
  {
    RoomName = $"{_gameManager.LocalPlayer.Name}的房间";
    _gameManager.PropertyChanged += (sender, e) =>
    {
      // 当 GameManager 的 SelectedMap 变化时，更新 ViewModel 的 _selectedMap
      if (e.PropertyName == nameof(GameManager.SelectedMap))
        SelectedMap = _gameManager.SelectedMap;
    };
    SelectedMap = _gameManager.SelectedMap;
  }


  [RelayCommand]
  private async Task CreateRoomAndNavigateToRoomPage()
  {
    if (_gameManager.SelectedMap == null)
    {
      await _dialogService.ShowDialogAsync(new MessageDialogViewModel
      {
        Title = "错误",
        Message = "无可用地图"
      });
      return;
    }

    _cts.TryReset();
    var netServer = App.Current.Services.GetRequiredService<NetServer>();
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new RoomPageViewModel(
      _gameManager.SelectedMap,
      App.Current.Services.GetRequiredService<NetClient>(),
      netServer,
      _gameManager,
      _cts));
    try
    {
      await netServer
        .CreateRoomAndListenAsync(RoomName, _gameManager.SelectedMap, _cts.Token);
    }
    catch (OperationCanceledException e)
    {
      await _dialogService.ShowDialogAsync(new MessageDialogViewModel
        { Title = "出现异常！", Message = e.Message });
    }
  }

  [RelayCommand]
  private async Task ShowSelectMapDialog()
  {
    await _dialogService.ShowDialogAsync(
      new SelectMapDialogViewModel(_mapFileService.GetMaps()));
  }
}