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
  private readonly GameManager _gameManager = App.Current.Services.GetRequiredService<GameManager>();

  [GeneratedDirectProperty] public partial ObservableCollection<String> Messages { get; set; } = [];
  
  public PlayerUI()
  {
    InitializeComponent();
    var playerUiViewModel = new PlayerUiViewModel
      { LocalPlayer = _gameManager.LocalPlayer };
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
      if (m.ChangedPlayer.Uuid == playerUiViewModel.LocalPlayer.Uuid)
      {
        UpdatePlayerInfo(_gameManager.LocalPlayer, m.ChangedPlayer);
        Messages.Add(m.UpdateMessage);
        return;
      }
      var player = playerUiViewModel.OtherPlayers.FirstOrDefault(p => p.Uuid == m.ChangedPlayer.Uuid);
      if (player != null)
      {
        UpdatePlayerInfo(player, m.ChangedPlayer);
        Messages.Add(m.UpdateMessage);
      }
    });
  }

  private void UpdatePlayerInfo(Player target, Player data)
  {
    target.Name = data.Name;
    target.Money = data.Money;
    target.Antiques = data.Antiques;
  }
}