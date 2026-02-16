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
  
  public PlayerUI()
  {
    InitializeComponent();
    var playerUiViewModel = new PlayerUiViewModel();
    DataContext = playerUiViewModel;
    
  }


}