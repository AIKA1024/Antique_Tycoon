using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Net;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class CreateRoomPageViewModel : PageViewModelBase
{
  [ObservableProperty] private string _roomName = $"{App.Current.Services.GetRequiredService<Player>().Name}的房间";

  [ObservableProperty]
  public partial Map SelectedMap { get; set; } = App.Current.Services.GetRequiredService<MapFileService>().GetMaps()[0];

  private CancellationTokenSource _cts = new();

  public CreateRoomPageViewModel()
  {
    WeakReferenceMessenger.Default.Register<ChangeMapMessage>(this, (_, m) => { SelectedMap = m.Value; });
  }

  [RelayCommand]
  private async Task CreateRoomAndNavigateToRoomPage()
  {
    _cts.TryReset();
    var netServer = App.Current.Services.GetRequiredService<NetServer>();
    netServer.MapStreamResolver = () => App.Current.Services.GetRequiredService<MapFileService>().GetMapFileStream(SelectedMap);
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new RoomPageViewModel(SelectedMap, _cts));
    try
    {
      await netServer
        .CreateRoomAndListenAsync(RoomName, SelectedMap, _cts.Token);
    }
    catch (OperationCanceledException e)
    {
      await App.Current.Services.GetRequiredService<DialogService>().ShowDialogAsync(new MessageDialogViewModel
        { Title = "出现异常！", Message = e.Message });
    }
  }

  [RelayCommand]
  private async Task ShowSelectMapDialog()
  {
    var services = App.Current.Services;
    await services.GetRequiredService<DialogService>()
      .ShowDialogAsync(new SelectMapDialogViewModel(services.GetRequiredService<MapFileService>().GetMaps()));
  }
}