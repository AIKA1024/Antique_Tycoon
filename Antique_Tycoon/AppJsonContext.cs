using System.Text.Json.Serialization;
using Antique_Tycoon.Models.Net;

namespace Antique_Tycoon;

[JsonSerializable(typeof(RoomNetInfo))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class AppJsonContext : JsonSerializerContext
{
}