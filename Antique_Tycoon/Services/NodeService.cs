using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Node;
using Antique_Tycoon.ViewModels.DialogViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace Antique_Tycoon.Services;

public class NodeService(DialogService dialogService)
{
    public async Task<Func<GameManager,Task>?> GetStepOnNodeHandlerAsync(NodeModel node, Player player)
    {
        switch (node)
        {
            case Estate estate:
                return await HandleEstateAsync(estate, player);
            case SpawnPoint:
                return await HandleSpawnPointAsync(player);
        }

        return null;
    }

    private async Task<Func<GameManager,Task>?> HandleEstateAsync(Estate estate, Player player)
    {
        if (estate.Owner == null && player.Money >= estate.Value)
        {
            bool isConfirm = await dialogService.ShowDialogAsync(new MessageDialogViewModel
            {
                Title = "是否购买该资产", Message = $"购买{estate.Title}需要{estate.Value}", IsShowCancelButton = true,
                IsLightDismissEnabled = false
            });
            if (isConfirm)
                return async gameManager =>
                {
                    if (gameManager.LocalPlayer.IsRoomOwner)
                    {
                        player.Money -= estate.Value;
                        var message = new UpdateEstateInfoResponse(player.Uuid, estate.Uuid);
                        await gameManager.NetServerInstance.Broadcast(message);
                        WeakReferenceMessenger.Default.Send(new UpdateEstateInfoMessage(player.Uuid,
                            estate.Uuid,1));
                    }
                    else
                    {
                        var message = new UpdateEstateInfoRequest(player.Uuid, estate.Uuid);//todo 服务器还没有处理这个消息
                        await gameManager.NetClientInstance.SendRequestAsync(message);
                    }
                };
        }

        return null;
    }

    private Task<Func<GameManager, Task>?> HandleSpawnPointAsync(Player player)
    {
        return Task.FromResult((GameManager gameManager) =>
        {
            player.Money += gameManager.SelectedMap.SpawnPointCashReward;
            return Task.CompletedTask;
        });
    }
}