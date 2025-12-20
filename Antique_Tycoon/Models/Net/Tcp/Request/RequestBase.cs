using System;
using System.Text.Json.Serialization;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

// [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
// [JsonDerivedType(typeof(BuyEstateRequest),  "BuyEstate")]//todo 后面传送等操作也要加进来
// [JsonDerivedType(typeof(DownloadMapRequest),  "DownloadMap")]
// [JsonDerivedType(typeof(ExitRoomRequest),  "ExitRoom")]
// [JsonDerivedType(typeof(HeartbeatMessage),  "Heartbeat")]
// [JsonDerivedType(typeof(JoinRoomRequest),  "JoinRoom")]
// [JsonDerivedType(typeof(PlayerMoveRequest),  "PlayerMove")]
// [JsonDerivedType(typeof(RollDiceRequest),  "RollDice")]
public abstract class RequestBase: ITcpMessage
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
}