using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Antique_Tycoon.Messages;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Entities;
using Antique_Tycoon.Models.Enums;
using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Models.Net.Tcp;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Net.Tcp.Response.GameAction;
using Antique_Tycoon.Models.Nodes;
using Antique_Tycoon.Services;
using Antique_Tycoon.ViewModels.DialogViewModels;
using Antique_Tycoon.ViewModels.PageViewModels;
using Avalonia.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using PropertyGenerator.Avalonia;

namespace Antique_Tycoon.ViewModels.ControlViewModels;

public partial class PlayerUiViewModel : PageViewModelBase, IDisposable
{
    private readonly GameManager _gameManager = App.Current.Services.GetRequiredService<GameManager>();

    private readonly ActionQueueService _actionQueueService =
        App.Current.Services.GetRequiredService<ActionQueueService>();

    [ObservableProperty] private bool _maskInvisible = true;
    [ObservableProperty] private Player _localPlayer;
    [ObservableProperty] private string _reminderText = "轮到你啦";
    [ObservableProperty] private bool _isShowReminderText;
    private string _rollDiceActionId = "";

    public DialogViewModelBase? DialogViewModel => _dialogService.CurrentDialogViewModel;

    [ObservableProperty] public partial bool RollButtonEnable { get; set; } = false;

    //PlayerUI自己的dialog服务，以实现按tab透明功能
    private readonly DialogService _dialogService = new();

    public ObservableCollection<Player> OtherPlayers { get; } = [];

    [ObservableProperty] public partial ObservableCollection<IHistoryRecord> HistoryLogs { get; set; } = [];


    public PlayerUiViewModel()
    {
        WeakReferenceMessenger.Default.Register<RollDiceAction>(this, ReceiveRollDiceAction);
        WeakReferenceMessenger.Default.Register<TurnStartResponse>(this, ReceiveTurnStartMessage);
        WeakReferenceMessenger.Default.Register<AntiqueChanceResponse>(this, ReceiveAntiqueChanceResponse);
        WeakReferenceMessenger.Default.Register<SaleAntiqueAction>(this, ReceiveSaleAntiqueAction);
        WeakReferenceMessenger.Default.Register<BuyEstateAction>(this, ReceiveBuyEstateAction);
        WeakReferenceMessenger.Default.Register<HireStaffAction>(this, ReceiveHireStaffAction);
        WeakReferenceMessenger.Default.Register<KeyPressedMessage>(this, ReceiveKeyPressedMessage);
        WeakReferenceMessenger.Default.Register<UpdateRoomResponse>(this, ReceiveUpdateRoomResponse);
        WeakReferenceMessenger.Default.Register<IHistoryRecord>(this, ReceiveIHistoryRecord);
        WeakReferenceMessenger.Default.Register<PlunderAntiqueAction>(this, ReceivePlunderAntiqueAction);
        LocalPlayer = _gameManager.LocalPlayer;
        _dialogService.DialogCollectionChanged += NotifyDialogViewModelChanged;
    }

    private void ReceivePlunderAntiqueAction(object recipient, PlunderAntiqueAction message)
    {
        _actionQueueService.Enqueue(async () =>
        {
            var vm = new PlunderAntiqueDialogViewModel(_gameManager.Players
                .Where(p => message.PlayerUuids.Any(s => p.Uuid == s)).ToList());

            var antique = await _dialogService.ShowDialogAsync(vm);
            await _gameManager.SendToGameServerAsync(new PlunderAntiqueRequest(message.Id, antique?.Uuid ?? ""));
        });
    }

    private void ReceiveIHistoryRecord(object recipient, IHistoryRecord message)
    {
        if (message.LogSegments.Count == 0)
            return;
        HistoryLogs.Add(message);
    }

    private void ReceiveKeyPressedMessage(object recipient, KeyPressedMessage message)
    {
        if (message.Value.Key == Key.Tab && _dialogService.CurrentDialogViewModel != null)
            MaskInvisible = !MaskInvisible;
    }

    private void ReceiveUpdateRoomResponse(object recipient, UpdateRoomResponse message)
    {
        foreach (var data in message.Players)
        {
            if (LocalPlayer.Uuid == data.Uuid)
            {
                UpdatePlayerInfo(LocalPlayer, data);
                continue;
            }

            var player = OtherPlayers.First(p => p.Uuid == data.Uuid);
            UpdatePlayerInfo(player, data);
        }
    }

    private void ReceiveHireStaffAction(object recipient, HireStaffAction message)
    {
        _actionQueueService.Enqueue(async () =>
        {
            var itemStacks = message.Staffs.GroupBy(s => s.GetType())
                .Select(group => new ItemStack<IStaff>(group.First(), group.Count())).ToArray();
            var selectStaff = await _dialogService.ShowDialogAsync(new HireStaffDialogViewModel(itemStacks));
            await _gameManager.SendToGameServerAsync(new HireStaffRequest(message.Id, selectStaff?.Uuid ?? ""));
        });
    }

    private void NotifyDialogViewModelChanged()
    {
        OnPropertyChanged(nameof(DialogViewModel));

        MaskInvisible = _dialogService.CurrentDialogViewModel == null;
    }

    public void Dispose()
    {
        _dialogService.DialogCollectionChanged -= NotifyDialogViewModelChanged;
        WeakReferenceMessenger.Default.Unregister<RollDiceAction>(this);
        WeakReferenceMessenger.Default.Unregister<TurnStartResponse>(this);
        WeakReferenceMessenger.Default.Unregister<AntiqueChanceResponse>(this);
        WeakReferenceMessenger.Default.Unregister<GetAntiqueResultResponse>(this);
        WeakReferenceMessenger.Default.Unregister<SaleAntiqueAction>(this);
        WeakReferenceMessenger.Default.Unregister<BuyEstateAction>(this);
        WeakReferenceMessenger.Default.Unregister<HireStaffAction>(this);
        WeakReferenceMessenger.Default.Unregister<KeyPressedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<UpdateRoomResponse>(this);
        WeakReferenceMessenger.Default.Unregister<IHistoryRecord>(this);
    }

    private void ReceiveBuyEstateAction(object recipient, BuyEstateAction message)
    {
        _actionQueueService.Enqueue(async () =>
        {
            var estate = (Estate)_gameManager.SelectedMap.EntitiesDict[message.EstateUuid];
            bool isConfirm = await _dialogService.ShowDialogAsync(new MessageDialogViewModel
            {
                Title = "是否购买该资产", Message = $"购买{estate.Title}需要{estate.Value}", IsShowCancelButton = true,
                IsLightDismissEnabled = false
            });
            BuyEstateRequest buyEstateRequest;
            if (isConfirm)
                buyEstateRequest = new BuyEstateRequest(message.Id, _gameManager.LocalPlayer.Uuid, estate.Uuid);
            else
                buyEstateRequest = new BuyEstateRequest { Id = message.Id, IsConfirm = false };

            await _gameManager.SendToGameServerAsync(buyEstateRequest);
        });
    }

    private void ReceiveSaleAntiqueAction(object recipient, SaleAntiqueAction message)
    {
        _actionQueueService.Enqueue(async () =>
        {
            var itemStacks = _gameManager.LocalPlayer.Antiques.GroupBy(a => a.Index)
                .Select(group => new ItemStack<Antique>(group.First(), group.Count())).OrderBy(s => s.Item.Value)
                .ToList();
            var estate = (Estate)_gameManager.SelectedMap.EntitiesDict[message.EstateUuid];
            var saleAntiqueDialogViewModel =
                new SaleAntiqueDialogViewModel(itemStacks, estate.CurrentLevel < estate.RevenueModifiers.Count)
                    { IsLightDismissEnabled = false };
            var saleAntiqueDetermination = await _dialogService.ShowDialogAsync(saleAntiqueDialogViewModel);
            if (saleAntiqueDetermination is null)
                await _gameManager.SendToGameServerAsync(new SaleAntiqueRequest(message.Id, "", false)); //空字符串代表不卖
            else
                await _gameManager.SendToGameServerAsync(new SaleAntiqueRequest(message.Id,
                    saleAntiqueDetermination.Antique.Uuid, saleAntiqueDetermination.NeedUpgrade));
        });
    }

    private void ReceiveAntiqueChanceResponse(object recipient, AntiqueChanceResponse message)
    {
        _actionQueueService.Enqueue(() =>
        {
            ReminderText = "再次投骰子以获取古玩";
            return Task.CompletedTask;
        });
    }

    private void ReceiveRollDiceAction(object recipient, RollDiceAction message)
    {
        _actionQueueService.Enqueue(() =>
        {
            RollButtonEnable = true;
            _rollDiceActionId = message.Id;
            return Task.CompletedTask;
        });
    }

    private void ReceiveTurnStartMessage(object sender, TurnStartResponse message)
    {
        IsShowReminderText = false;
        if (message.PlayerUuid == _gameManager.LocalPlayer.Uuid)
        {
            ReminderText = "轮到你啦";
            IsShowReminderText = true;
        }
    }

    [RelayCommand]
    private async Task RollDiceAsync()
    {
        Debug.WriteLine("发起投骰子请求");
        RollButtonEnable = false;
        await _gameManager.SendToGameServerAsync(new RollDiceRequest(_rollDiceActionId));
        Debug.WriteLine("请求结束");
    }

    [RelayCommand]
    private void HandleSegmentClick(LogSegment segment)
    {
        switch (segment.Type)
        {
            case InteractionType.PlayerName:
                Debug.WriteLine($"查看玩家: {segment.Data}");
                break;
            case InteractionType.Antique:
                Debug.WriteLine($"查看古玩: {segment.Data}");
                break;
            // ... 其他逻辑
        }
    }

    private void UpdatePlayerInfo(Player target, Player data)
    {
        target.Name = data.Name;
        target.Money = data.Money;
        target.Antiques = data.Antiques;
        target.Staffs = data.Staffs;
        target.Estates = data.Estates;
    }
}