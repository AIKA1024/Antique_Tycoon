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
  StartGameResponse = 6,
  DownloadMapRequest = 7,
  DownloadMapResponse = 8,
  TurnStartResponse = 9,
  RollDiceRequest = 10,
  RollDiceResponse = 11,
  InitGameMessageResponse,
  PlayerMoveRequest,
  PlayerMoveResponse,
}