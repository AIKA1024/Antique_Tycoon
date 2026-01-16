using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Antique_Tycoon.Net;

public class NetClient : NetBase
{
    private readonly UdpClient _udpClient = new();
    private TcpClient? _tcpClient;
    private readonly Dictionary<string, TaskCompletionSource<ITcpMessage>> _pendingRequests = new();
    private readonly GameManager _gameManager;
    public TimeSpan HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(3);

    public NetClient(GameManager gameManagerLazy, string downloadPath)
    {
        _gameManager = gameManagerLazy;
        DownloadPath = downloadPath;
    }

    public async Task<RoomBaseInfo> DiscoverRoomAsync()
    {
        _udpClient.EnableBroadcast = true;
        var bytes = "DiscoverRoom"u8.ToArray();
        await _udpClient.SendAsync(bytes, bytes.Length, "255.255.255.255", App.DefaultPort);
        var result = await _udpClient.ReceiveAsync();
        var json = Encoding.UTF8.GetString(result.Buffer);
        var roomInfo = JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.RoomBaseInfo);
        return roomInfo ?? throw new Exception("Could not deserialize room info");
    }

    private async Task HeartbeatLoopAsync(CancellationToken cancellation = default)
    {
        while (!cancellation.IsCancellationRequested)
        {
            try
            {
                _ = SendRequestAsync(new HeartbeatMessage { PlayerUuid = _gameManager.LocalPlayer.Uuid }, cancellation);
                await Task.Delay(HeartbeatInterval, cancellation);
            }
            catch (Exception ex)
            {
#if DEBUG
                await App.Current.Services.GetRequiredService<DialogService>().ShowDialogAsync(
                    new MessageDialogViewModel
                        { Title = "警告", Message = ex.Message, IsLightDismissEnabled = false });
#endif
                break;
            }
        }
    }

    public async Task ConnectServer(IPEndPoint ipEndPoint, CancellationToken cancellation = default)
    {
        _tcpClient?.Dispose();
        _tcpClient = new TcpClient();
        await _tcpClient.ConnectAsync(ipEndPoint, cancellation);
        _ = ReceiveLoopAsync(_tcpClient, cancellation);
        _ = HeartbeatLoopAsync(cancellation); // 开始循环发送心跳包
    }

    #region 封装的发送和接收逻辑

    public async Task<ITcpMessage> SendRequestAsync<T>(T message, CancellationToken cancellationToken = default)
        where T : ITcpMessage
    {
        var data = PackMessage(message);
        var tcs = new TaskCompletionSource<ITcpMessage>();
        _pendingRequests[message.Id] = tcs;
        await _tcpClient.GetStream().WriteAsync(data, cancellationToken);
        // 等待服务器响应（HandleReceive 中会完成 tcs）
        return await tcs.Task.WaitAsync(cancellationToken);
    }

    protected override Task ProcessMessageAsync(TcpMessageType tcpMessageType, string json, TcpClient client)
    {
        ITcpMessage? response = null;
        switch (tcpMessageType)
        {
            case TcpMessageType.JoinRoomResponse:
                var joinRoomResponse = JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.JoinRoomResponse);
                response = joinRoomResponse;
                break;
            case TcpMessageType.UpdateRoomResponse:
                var updateRoomResponse = JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.UpdateRoomResponse);
                response = updateRoomResponse;
                WeakReferenceMessenger.Default.Send(updateRoomResponse);
                break;
            case TcpMessageType.DownloadMapResponse:
                var downloadMapResponse = JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.DownloadMapResponse);
                response = downloadMapResponse;
                break;
            case TcpMessageType.StartGameResponse:
                var startGameResponse = JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.StartGameResponse);
                response = startGameResponse;
                WeakReferenceMessenger.Default.Send(startGameResponse);
                break;
            case TcpMessageType.TurnStartResponse:
                var turnStartResponse = JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.TurnStartResponse);
                response = turnStartResponse;
                WeakReferenceMessenger.Default.Send(turnStartResponse);
                break;
            case TcpMessageType.RollDiceAction:
                var rollDiceAction = JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.RollDiceAction);
                response = rollDiceAction;
                WeakReferenceMessenger.Default.Send(rollDiceAction);
                break;
            case TcpMessageType.RollDiceResponse:
                var rollDiceResponse = JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.RollDiceResponse);
                response = rollDiceResponse;
                WeakReferenceMessenger.Default.Send(rollDiceResponse);
                break;
            case TcpMessageType.InitGameResponse:
                var initGameMessageResponse =
                    JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.InitGameResponse);
                response = initGameMessageResponse;
                WeakReferenceMessenger.Default.Send(initGameMessageResponse);
                break;
            case TcpMessageType.SelectDestinationAction:
                var selectDestinationAction = JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.SelectDestinationAction);
                response = selectDestinationAction;
                WeakReferenceMessenger.Default.Send(selectDestinationAction);
                break;
            case TcpMessageType.PlayerMoveResponse:
                var playerMoveResponse = JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.PlayerMoveResponse);
                response = playerMoveResponse;
                WeakReferenceMessenger.Default.Send(playerMoveResponse);
                break;
            case TcpMessageType.BuyEstateAction:
                var buyEstateAction = JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.BuyEstateAction);
                response = buyEstateAction;
                WeakReferenceMessenger.Default.Send(buyEstateAction);
                break;
            case TcpMessageType.UpdateEstateInfoResponse:
                var updateEstateOwnerResponse =
                    JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.UpdateEstateInfoResponse);
                response = updateEstateOwnerResponse;
                WeakReferenceMessenger.Default.Send(updateEstateOwnerResponse);
                break;
            case TcpMessageType.UpdatePlayerInfoResponse:
                var updatePlayerInfoResponse =
                    JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.UpdatePlayerInfoResponse);
                response = updatePlayerInfoResponse;
                WeakReferenceMessenger.Default.Send(updatePlayerInfoResponse);
                break;
            case TcpMessageType.AntiqueChanceResponse:
                var antiqueChanceResponse =
                    JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.AntiqueChanceResponse);
                response = antiqueChanceResponse;
                WeakReferenceMessenger.Default.Send(antiqueChanceResponse,antiqueChanceResponse.MineUuid);
                break;
            case TcpMessageType.GetAntiqueResultResponse:
                var getAntiqueResultResponse =
                    JsonSerializer.Deserialize(json, Models.Json.AppJsonContext.Default.GetAntiqueResultResponse);
                response = getAntiqueResultResponse;
                WeakReferenceMessenger.Default.Send(getAntiqueResultResponse);
                break;
        }

        if (response == null)
            return Task.CompletedTask;
        if (!_pendingRequests.Remove(response.Id, out var tcs)) //证明不是客户端主动请求的，是服务器主动发送的
            return Task.CompletedTask;


        tcs.SetResult(response);
        return tcs.Task;
    }

    protected override async Task ReceiveFileChunkAsync(string uuid, string fileName, int chunkIndex, int totalChunks,
        byte[] data)
    {
        await base.ReceiveFileChunkAsync(uuid, fileName, chunkIndex, totalChunks, data);
        if (_pendingRequests.TryGetValue(uuid, out var tcs))
        {
            tcs.SetResult(new DownloadMapResponse { Id = uuid });
            _pendingRequests.Remove(uuid);
        }
    }

    #endregion
}