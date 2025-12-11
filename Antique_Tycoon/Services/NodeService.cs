using System;
using System.Threading.Tasks;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Node;
using Antique_Tycoon.ViewModels.DialogViewModels;

namespace Antique_Tycoon.Services;

public class NodeService(DialogService dialogService)
{
    public async Task<Func<GameManager,Task>?> HandleStepOnNodeLocalAsync(NodeModel node, Player player)
    {
        switch (node)
        {
            case Estate estate:
                return await HandleEstateAsync(estate, player);
            case SpawnPoint:
                break;
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
                    }
                    else
                    {
                        var message = new UpdateEstateInfoRequest(player.Uuid, estate.Uuid);
                        await gameManager.NetClientInstance.SendRequestAsync(message);
                    }
                };
        }

        return null;
    }
}