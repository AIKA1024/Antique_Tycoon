using System;
namespace Antique_Tycoon.Models.Net.Tcp.Request;

public class JoinRoomRequest : IGameMessage
{
  public string Id { get; set; } = Guid.NewGuid().ToString();
  public long Timestamp { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
  public required Player Player { get; set; }
  public string PlayerUuid => Player.Uuid;
}