using System;
using System.Collections.ObjectModel;
using System.Linq;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.ControlViewModels;
using Avalonia.Controls;
using Avalonia.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.Views.Widgets;

public partial class PlayerUI : UserControl
{
  
  [GeneratedDirectProperty]
  public partial ObservableCollection<String> Messages { get; set; }
  
  public PlayerUI()
  {
    InitializeComponent();
    var playerUiViewModel = new PlayerUiViewModel
      { LocalPlayer = App.Current.Services.GetRequiredService<GameManager>().LocalPlayer };
    DataContext = playerUiViewModel;
    WeakReferenceMessenger.Default.Register<KeyPressedMessage>(this, (_, m) =>
    {
      if (m.Value.Key == Key.Tab)
        playerUiViewModel.IsVisible = !playerUiViewModel.IsVisible;
    });
    WeakReferenceMessenger.Default.Register<UpdateRoomResponse>(this, (_, m) =>
    {
      foreach (var data in m.Players)
      {
        if (playerUiViewModel.LocalPlayer.Uuid == data.Uuid)
        {
          UpdatePlayerInfo(playerUiViewModel.LocalPlayer, data);
          continue;
        }

        var player = playerUiViewModel.OtherPlayers.First(p => p.Uuid == data.Uuid);
        UpdatePlayerInfo(player, data);
      }
    });
    WeakReferenceMessenger.Default.Register<UpdatePlayerInfoResponse>(this, (_, m) =>
    {
      var player = playerUiViewModel.OtherPlayers.First(p => p.Uuid == m.ChangedPlayer.Uuid);
      UpdatePlayerInfo(player, m.ChangedPlayer);
      Messages.Add(m.UpdateMessage);
    });
  }

  private void UpdatePlayerInfo(Player target, Player data)
  {
    target.Name = data.Name;
    target.Money = data.Money;
    target.Antiques = data.Antiques;
  }
}