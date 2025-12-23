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
  BuyEstateAction,
  BuyEstateRequest,
  UpdateEstateInfoResponse,
  UpdatePlayerInfoResponse,
  ExitRoomRequest,
  ExitRoomResponse,
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