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
  RollDiceAction,
  RollDiceRequest,
  RollDiceResponse,
  InitGameMessageResponse,
  PlayerMoveRequest,
  PlayerMoveResponse,
  SelectDestinationAction,
  SelectDestinationRequest,
  AntiqueChanceResponse,
  GetAntiqueResultResponse
}