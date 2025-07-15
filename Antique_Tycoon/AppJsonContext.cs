using System.Text.Json.Serialization;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Models.Net.Tcp;

namespace Antique_Tycoon;

[JsonSerializable(typeof(Player))]
[JsonSerializable(typeof(RoomBaseInfo))]
[JsonSerializable(typeof(JoinRoomResponse))]
[JsonSerializable(typeof(JoinRoomRequest))]
[JsonSerializable(typeof(UpdateRoomResponse))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class AppJsonContext : JsonSerializerContext
{
}