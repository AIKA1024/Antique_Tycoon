namespace Antique_Tycoon.Models.Net.Tcp;

public enum TcpMessageType : ushort
{
  /// <summary>
  /// 心跳
  /// </summary>
  HeartbeatMessage = 1,
  JoinRoomRequest = 2,
  JoinRoomResponse = 3,
  UpdateRoomResponse = 4,
  ExitRoomRequest = 5,
  StartGameResponse = 6
}