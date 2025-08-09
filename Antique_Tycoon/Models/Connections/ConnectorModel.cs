using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Antique_Tycoon.Models.Connections;

public partial class ConnectorModel : ObservableObject
{
  [ObservableProperty] public partial string Uuid { get; set; } = Guid.NewGuid().ToString();
  [ObservableProperty] public partial Point Anchor { get; set; }

  [JsonIgnore]
  public List<Connection> ActiveConnections { get; set; } = [];
  [JsonIgnore]
  public List<Connection> PassiveConnections { get; set; } = [];
}