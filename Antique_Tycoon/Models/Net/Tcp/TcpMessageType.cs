namespace Antique_Tycoon.Models.Net.Tcp;

public enum TcpMessageType : ushort
{
  /// <summary>
  /// 心跳
  /// </summary>
  HeartbeatMessage,
  JoinRoomRequest,
  JoinRoomResponse,
  UpdateRoomResponse,
  UpdateEstateInfoRequest,
  UpdateEstateInfoResponse,
  ExitRoomRequest,
  StartGameResponse,
  DownloadMapRequest,
  DownloadMapResponse,
  TurnStartResponse,
  RollDiceRequest,
  RollDiceResponse,
  InitGameMessageResponse,
  PlayerMoveRequest,
  PlayerMoveResponse,
}