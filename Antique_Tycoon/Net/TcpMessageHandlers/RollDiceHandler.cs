// using System;
// using System.Net.Sockets;
// using System.Text.Json;
// using System.Threading.Tasks;
// using System.Threading;
// using Antique_Tycoon.Messages;
// using Antique_Tycoon.Models.Net.Tcp;
// using Antique_Tycoon.Models.Net.Tcp.Request;
// using Antique_Tycoon.Models.Net.Tcp.Response;
// using Antique_Tycoon.Services;
// using Avalonia.Utilities;
// using CommunityToolkit.Mvvm.Messaging;
//
// namespace Antique_Tycoon.Net.TcpMessageHandlers;
//
// public class RollDiceHandler(GameManager gameManager) : ITcpMessageHandler
// {
//     private readonly SemaphoreSlim _rollDiceLock = gameManager.GameActionLock; 
//
//     public bool CanHandle(TcpMessageType messageType)
//     {
//         return messageType == TcpMessageType.RollDiceRequest;
//     }
//
//     public async Task HandleAsync(string json, TcpClient client)
//     {
//         // 1. 先反序列化请求，失败直接返回
//         var rollDiceRequest = JsonSerializer.Deserialize(json, AppJsonContext.Default.RollDiceRequest);
//         if (rollDiceRequest == null)
//         {
//             throw new JsonException("解析RollDiceRequest失败");
//         }
//
//         string clientPlayerUuid = gameManager.GetPlayerUuidByTcpClient(client);
//         
//         // 2. 第一层校验：非当前玩家直接拒绝（快速失败）
//         if (gameManager.CurrentTurnPlayer.Uuid != clientPlayerUuid)
//         {
//             await SendRejectResponseAsync(rollDiceRequest.Id, client);
//             return;
//         }
//
//         // 3. 异步锁：防止同一玩家重复投骰子（3秒超时，避免死等）
//         bool lockAcquired = false;
//         try
//         {
//             lockAcquired = await _rollDiceLock.WaitAsync(TimeSpan.FromSeconds(3));
//             if (!lockAcquired)
//             {
//                 // 锁超时：拒绝重复请求
//                 await SendRejectResponseAsync(rollDiceRequest.Id, client);
//                 return;
//             }
//
//             // 4. 第二层校验：锁内再次校验（防止等待期间玩家已切换）
//             if (gameManager.CurrentTurnPlayer?.Uuid != clientPlayerUuid)
//             {
//                 await SendRejectResponseAsync(rollDiceRequest.Id, client);
//                 return;
//             }
//
//             // 5. 执行投骰子核心逻辑（原有逻辑保留）
//             int value = Random.Shared.Next(1, 7);
//             var response = new RollDiceResponse(rollDiceRequest.Id, clientPlayerUuid, value)
//             {
//                 ResponseStatus = RequestResult.Success
//             };
//             await gameManager.NetServerInstance.Broadcast(response);
//             WeakReferenceMessenger.Default.Send(response);
//         }
//         catch (Exception ex)
//         {
//             // 异常处理：记录日志并返回错误
//             Console.WriteLine($"处理投骰子请求失败：{ex.Message}");
//             await SendRejectResponseAsync(rollDiceRequest.Id, client);
//         }
//         finally
//         {
//             // 6. 释放锁：只有成功获取锁才释放，避免重复释放
//             if (lockAcquired)
//             {
//                 _rollDiceLock.Release();
//             }
//         }
//     }
//
//     /// <summary>
//     /// 封装拒绝响应的发送逻辑，简化代码
//     /// </summary>
//     private async Task SendRejectResponseAsync(string requestId, TcpClient client)
//     {
//         var rejectResponse = new RollDiceResponse(requestId, "", 0)
//         {
//             ResponseStatus = RequestResult.Reject,
//         };
//         await gameManager.NetServerInstance.SendResponseAsync(rejectResponse, client);
//     }
// }