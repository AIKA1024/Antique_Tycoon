using System;
using System.Threading.Tasks;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Node;
using Antique_Tycoon.ViewModels.DialogViewModels;

namespace Antique_Tycoon.Services;

public class NodeService(DialogService dialogService)
{
    public async Task<Action<GameManager>?> HandleStepOnNodeLocalAsync(NodeModel node, Player player)
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

    private async Task<Action<GameManager>?> HandleEstateAsync(Estate estate, Player player)
    {
        if (estate.Owner==null && player.Money >= estate.Value)
        { 
            bool result = await dialogService.ShowDialogAsync(new MessageDialogViewModel
                { Title = "是否购买该资产", Message = $"购买{estate.Title}需要{estate.Value}",IsShowCancelButton = true,IsLightDismissEnabled = false});
            if (result)
                return gameManager =>
                {
                    if (gameManager.LocalPlayer.IsRoomOwner)
                    {
                        //gameManager.NetServerInstance.Broadcast() todo 广播购买地产
                    }
                };
        }
        return null;
    }
}