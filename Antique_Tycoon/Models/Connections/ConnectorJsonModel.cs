using Antique_Tycoon.Converters.JsonConverter;
using Avalonia;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Antique_Tycoon.Models.Connections;

public partial class ConnectorJsonModel : ObservableObject
{
  [ObservableProperty] public partial string Uuid { get; set; } = Guid.NewGuid().ToString();
  [ObservableProperty]
  [JsonConverter(typeof(PointJsonConverter))]
  public partial Point Anchor { get; set; }

  [JsonIgnore]
  public List<Connection> ActiveConnections { get; set; } = [];
  [JsonIgnore]
  public List<Connection> PassiveConnections { get; set; } = [];
}