using System.Text.Json.Serialization;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Connections;
using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Node;
using Antique_Tycoon.Models.RoleBehaviors;
using Avalonia.Media;
using Player = Antique_Tycoon.Models.Player;

namespace Antique_Tycoon;
[JsonSerializable(typeof(Player))]
[JsonSerializable(typeof(RoomBaseInfo))]
[JsonSerializable(typeof(HeartbeatMessage))]
[JsonSerializable(typeof(JoinRoomResponse))]
[JsonSerializable(typeof(JoinRoomRequest))]
[JsonSerializable(typeof(UpdateRoomResponse))]
[JsonSerializable(typeof(ExitRoomRequest))]
[JsonSerializable(typeof(StartGameResponse))]
[JsonSerializable(typeof(DownloadMapRequest))]
[JsonSerializable(typeof(DownloadMapResponse))]
[JsonSerializable(typeof(TurnStartResponse))]
[JsonSerializable(typeof(RollDiceRequest))]
[JsonSerializable(typeof(RollDiceResponse))]
[JsonSerializable(typeof(InitGameMessageResponse))]
[JsonSerializable(typeof(PlayerMoveRequest))]
[JsonSerializable(typeof(PlayerMoveResponse))]

[JsonSerializable(typeof(Map))]
[JsonSerializable(typeof(SpawnPoint))]
[JsonSerializable(typeof(Estate))]
[JsonSerializable(typeof(Connection))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class AppJsonContext : JsonSerializerContext;