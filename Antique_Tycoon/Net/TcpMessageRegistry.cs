using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization.Metadata;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Net.Tcp.Response.GameAction;

namespace Antique_Tycoon.Net;

public static class TcpMessageRegistry
{
    private static readonly Dictionary<TcpMessageType, TcpMessageMetadata> _byMessageType;
    private static readonly Dictionary<Type, TcpMessageMetadata> _byClrType;

    static TcpMessageRegistry()
    {
        var list = new[]
        {
            // -------- Requests --------
            Create(TcpMessageType.JoinRoomRequest, typeof(JoinRoomRequest), AppJsonContext.Default.JoinRoomRequest),
            Create(TcpMessageType.ExitRoomRequest, typeof(ExitRoomRequest), AppJsonContext.Default.ExitRoomRequest),
            Create(TcpMessageType.DownloadMapRequest, typeof(DownloadMapRequest), AppJsonContext.Default.DownloadMapRequest),
            Create(TcpMessageType.RollDiceRequest, typeof(RollDiceRequest), AppJsonContext.Default.RollDiceRequest),
            Create(TcpMessageType.PlayerMoveRequest, typeof(PlayerMoveRequest), AppJsonContext.Default.PlayerMoveRequest),
            Create(TcpMessageType.BuyEstateRequest, typeof(BuyEstateRequest), AppJsonContext.Default.BuyEstateRequest),
            Create(TcpMessageType.SelectDestinationRequest, typeof(SelectDestinationRequest), AppJsonContext.Default.SelectDestinationRequest),

            // -------- Responses --------
            Create(TcpMessageType.JoinRoomResponse, typeof(JoinRoomResponse), AppJsonContext.Default.JoinRoomResponse),
            Create(TcpMessageType.UpdateRoomResponse, typeof(UpdateRoomResponse), AppJsonContext.Default.UpdateRoomResponse),
            Create(TcpMessageType.StartGameResponse, typeof(StartGameResponse), AppJsonContext.Default.StartGameResponse),
            Create(TcpMessageType.RollDiceResponse, typeof(RollDiceResponse), AppJsonContext.Default.RollDiceResponse),
            Create(TcpMessageType.PlayerMoveResponse, typeof(PlayerMoveResponse), AppJsonContext.Default.PlayerMoveResponse),
            Create(TcpMessageType.DownloadMapResponse, typeof(DownloadMapResponse), AppJsonContext.Default.DownloadMapResponse),
            Create(TcpMessageType.TurnStartResponse, typeof(TurnStartResponse), AppJsonContext.Default.TurnStartResponse),
            Create(TcpMessageType.InitGameMessageResponse, typeof(InitGameResponse), AppJsonContext.Default.InitGameResponse),
            Create(TcpMessageType.ExitRoomResponse, typeof(ExitRoomResponse), AppJsonContext.Default.ExitRoomResponse),
            Create(TcpMessageType.UpdateEstateInfoResponse, typeof(UpdateEstateInfoResponse), AppJsonContext.Default.UpdateEstateInfoResponse),
            Create(TcpMessageType.UpdatePlayerInfoResponse, typeof(UpdatePlayerInfoResponse), AppJsonContext.Default.UpdatePlayerInfoResponse),
            Create(TcpMessageType.AntiqueChanceResponse, typeof(AntiqueChanceResponse), AppJsonContext.Default.AntiqueChanceResponse),
            Create(TcpMessageType.GetAntiqueResultResponse, typeof(GetAntiqueResultResponse), AppJsonContext.Default.GetAntiqueResultResponse),

            // -------- Actions --------
            Create(TcpMessageType.RollDiceAction, typeof(RollDiceAction), AppJsonContext.Default.RollDiceAction),
            Create(TcpMessageType.BuyEstateAction, typeof(BuyEstateAction), AppJsonContext.Default.BuyEstateAction),
            Create(TcpMessageType.SelectDestinationAction, typeof(SelectDestinationAction), AppJsonContext.Default.SelectDestinationAction),

            // -------- Misc --------
            Create(TcpMessageType.HeartbeatMessage, typeof(HeartbeatMessage), AppJsonContext.Default.HeartbeatMessage),
        };

        _byMessageType = list.ToDictionary(x => x.MessageType);
        _byClrType     = list.ToDictionary(x => x.ClrType);
    }

    private static TcpMessageMetadata Create(
        TcpMessageType msgType,
        Type clrType,
        JsonTypeInfo jsonTypeInfo)
    {
        return new TcpMessageMetadata(msgType, clrType, jsonTypeInfo);
    }

    // ---------- 对外 API ----------

    public static TcpMessageMetadata ByMessageType(TcpMessageType type)
        => _byMessageType[type];

    public static TcpMessageMetadata ByClrType(Type type)
        => _byClrType[type];
}
