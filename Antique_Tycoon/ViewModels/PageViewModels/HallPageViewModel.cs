using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Timers;
using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Net;
using Antique_Tycoon.Services;
using Avalonia.Collections;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.ViewModels.PageViewModels;

public partial class HallPageViewModel:PageViewModelBase
{
  private readonly Timer _timer = new(2000);
  public AvaloniaList<RoomBaseInfo> RoomList { get; } = [];
  // public AvaloniaList<RoomBaseInfo> RoomList { get; } = [new RoomBaseInfo{Ip = "127.0.0.1",Port = 13437,RoomName = "lbw的房间"}];
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
    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new PageViewModels.CreateRoomPageViewModel());
  }

  [RelayCommand]
  private async Task JoinRoom(RoomBaseInfo roomInfo)
  {
    var iPEndPoint = new IPEndPoint(IPAddress.Parse(roomInfo.Ip), roomInfo.Port);
    await App.Current.Services.GetRequiredService<NetClient>().ConnectServer(iPEndPoint);
    var response = await App.Current.Services.GetRequiredService<NetClient>().JoinRoomAsync();
    if (response.Players.Count == 0)
    {
      return;//todo 要显示提示
    }
    

    App.Current.Services.GetRequiredService<NavigationService>().Navigation(new RoomPageViewModel
    {
      Players = response.Players
    });
  }
}