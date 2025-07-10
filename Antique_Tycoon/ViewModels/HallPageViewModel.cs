using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Net;
using Antique_Tycoon.Services;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels;

public partial class HallPageViewModel:ViewModelBase
{
  private readonly Timer _timer = new(2000);
  public AvaloniaList<RoomNetInfo> RoomList { get; } = [];
  // public AvaloniaList<RoomNetInfo> RoomList { get; } = [new RoomNetInfo{Ip = "127.0.0.1",Port = 13437,RoomName = "lbw的房间"}];
  public HallPageViewModel()
  {
    _timer.Elapsed += async (s, e) =>
    {
      await UpdateRoomList(s, e);
    };
    _timer.Start();
  }

  private async Task UpdateRoomList(object? sender, ElapsedEventArgs e)
  {
    var roomNetInfo = await App.Current.Services.GetRequiredService<NetClient>().DiscoverRoomAsync();
    if (RoomList.Any(r=>Equals(r.Ip, roomNetInfo.Ip)))
      return;
    RoomList.Add(roomNetInfo);
  }
  
  [RelayCommand]
  private void NavigateToCreateRoomPage()
  {
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new CreateRoomPageViewModel());
  }

  [RelayCommand]
  private void JoinRoom()
  {
    
  }
}