# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Build & Run

```bash
# Build and run (Debug)
dotnet run --project Antique_Tycoon/Antique_Tycoon.csproj

# Build and run (Release, with NativeAOT)
dotnet run --project Antique_Tycoon/Antique_Tycoon.csproj -c Release

# Build with the "Debug发布" custom configuration (Debug + TRACE)
dotnet build -c "Debug发布"

# Build both projects
dotnet build Antique_Tycoon.sln
```

There are three solution configurations: `Debug`, `Release`, and `Debug发布` (Debug symbols + TRACE). Release uses NativeAOT (`PublishAot=true`). The app is Windows-only (`RuntimeIdentifier=win-x64`). The C# language version is `preview`.

No tests exist for this project.

## Solution Structure

```
Antique_Tycoon.sln
├── Antique_Tycoon/              # Main Avalonia desktop app (net10.0, win-x64)
└── Antique_Tycoon.ProtocolGen/  # Roslyn incremental source generator (netstandard2.0)
```

### ProtocolGen — Source Generator for Network Messages

`Antique_Tycoon.ProtocolGen` is an **incremental source generator** (`IIncrementalGenerator`). It scans all classes decorated with `[TcpMessage]` and generates:

1. **`TcpMessageType.g.cs`** — Enum mapping each message class to a `ushort` ID
2. **`TcpMessageRegistry.g.cs`** — Static registry with `Get(Type)` / `Get(TcpMessageType)` lookups and a `Dispatch(ITcpMessage)` method that uses a switch on concrete types (AOT-friendly, no reflection)

Messages live under `Antique_Tycoon/Models/Net/Tcp/Request/` and `Response/`. To add a new TCP message type, mark the class with `[TcpMessage]`.

## High-Level Architecture

### MVVM + DI

The app uses **Avalonia UI** with **CommunityToolkit.Mvvm** (source generators: `[ObservableProperty]`, `[RelayCommand]`). The DI container is built in `App.axaml.cs:ConfigureServices()` using `Microsoft.Extensions.DependencyInjection`. Key Singletons: `GameManager`, `GameRuleService`, `NavigationService`, `DialogService`, `SoundService`, `NetClient`, `NetServer`.

`MainWindow` uses a `TransitioningContentControl` bound to `MainWindowViewModel.CurrentPageViewModel` for page navigation. `MainWindowViewModel` extends `PageViewModelBase`, which provides lifecycle hooks (`OnNavigatedTo`, `OnNavigatingFrom`, `OnBacked`).

### Messaging

**`WeakReferenceMessenger`** (CommunityToolkit) is the pub-sub bus. TCP response messages from `TcpMessageRegistry.Dispatch()` are dispatched as typed messages on this bus — GameManager, GameRuleService, and ViewModels subscribe to the ones they care about. Dialog return values also flow through typed `WeakReferenceMessenger.Send()` / `await message.Response` patterns.

### Page Navigation

`NavigationService` manages a `List<PageViewModelBase>` history stack. Pages: `StartPage` → `HallPage` / `CreateRoomPage` → `RoomPage` → `GamePage`. `MapEditPage` opens from `MapListPage`. `GamePageViewModel` receives the selected `Map` in its constructor.

### Game Data Model

- **`Map`** (in `Models/Map.cs`) — Contains `Entities` (an `ObservableCollection<CanvasItemModel>`) and a fast-lookup `EntitiesDict`. Nodes are subtypes of `NodeModel` (abstract, extends `CanvasItemModel`): `Estate`, `SpawnPoint`, `Mine`, `TalentMarket`, `TeleportationPoint`, `EnderChest`, `Ender`.
- **`Player`** — ObservableObject with `Money`, `Antiques`, `Staffs`, `Estates`, `Role` (Minecraft-themed avatars), and `CurrentNodeUuId`.
- **`Antique`** / **`IStaff`** — Game entities with effects. Staff implement `IStaff` + `IStaffEffect` for triggered game effects.
- **`GameTriggerPoint`** enum — lifecycle hooks (`OnPassStartPoint`, `OnAppraisalRoll`, `OnCalculateTax`, etc.) where staff effects fire.

### Network Architecture

**Dual-protocol**: TCP for game messages, UDP for LAN room discovery.

- **`NetBase`** — Shared TCP logic: binary framing (4-byte length prefix + 2-byte `TcpMessageType` + JSON payload), file chunk transfer. `ReceiveLoopAsync` reads from a `MemoryStream` buffer.
- **`NetClient`** — Connects to server, sends heartbeat every 3s, maps request IDs to `TaskCompletionSource<ITcpMessage>` for async request-response.
- **`NetServer`** — Listens for TCP connections via `TcpListener`, detects timeouts, handles UDP discovery broadcasts (`HandleUdpDiscoveryAsync`). Uses `ITcpMessageHandler` strategy pattern for routing incoming non-response messages (e.g., `JoinRoomHandler`, `ExitRoomHandler`, `DownloadMapHandler`, `PlayerMoveHandler`, `RollDiceHandler`).
- **Loopback optimization**: When the host player sends a request, `GameManager.SendToGameServerAsync` detects that `IsRoomOwner` is true and calls `NetServer.ReceiveLocalMessage()` directly instead of going through TCP.

### Core Services

- **`GameManager`** — Central game state: `Players` list (via `ObservableDictionary`), `SelectedMap`, turn tracking, room ownership. Handles player join/leave, map downloading, and initiation of game start. Listens to network responses via `WeakReferenceMessenger` to update state.
- **`GameRuleService`** — Turn-based game logic loop (`StartGameRule`): dice roll → path selection → node stepping → advance to next player. Each node type (`Estate`, `Mine`, `TalentMarket`, etc.) has its own handler. Staff effects (`IStaffEffect`) are triggered at `GameTriggerPoint`s.
- **`DialogService`** — Modal dialog stack with async result patterns (`ShowDialogAsync<T>` returns `Task<T?>`). Dialogs are `DialogViewModelBase` subtypes rendered via `DataTemplates/DialogDataTemplate/`.
- **`PersistenceService`** — JSON config persistence to `../Configs/` directory (e.g., `PlayerConfig`, `MainWindowConfig`).
- **`ActionQueueService`** — Sequential async action queue for UI animations (e.g., player movement animation plays before updating position).
- **`SoundService`** — LibVLC-based SFX/BGM with role-strategy sound effects (`CowSound`, `ZombieSound`, etc.).

### Views Organization

- `Views/Windows/` — Top-level windows (`MainWindow`, `DebugWindow`)
- `Views/Widgets/` — Reusable composite controls (`GameCanvas`, `PlayerUI`, `MapCard`)
- `Views/Controls/` — Custom controls (`MCButton`, `MCTitleBar`, `MasterDetailView`, `Connector`, `PlayerAvatar`, etc.)
- `Views/DataTemplates/` — DataTemplate `.axaml` files matched to ViewModels by convention (no `DataTemplate` `x:Key` needed; Avalonia resolves by data type). Subfolders: `PagesDataTemplate/`, `DialogDataTemplate/`, `NodeDataTemplate/`, `DetailDataTemplate/`
- `Views/ControlThemes/` — Control theme overrides (e.g., `TabItemTheme`)
- `Views/Styles/` — Style overrides for built-in controls
- `Behaviors/` — Avalonia behaviors (`ZoomPanBehavior`, `CanvasItemDragBehavior`, `ShowFlyoutBehavior`, etc.)
- `Converters/` — `IValueConverter` / `IMultiValueConverter` implementations
- `Extensions/` — Markup extensions (`EnumItemsExtension`, `BitmapExtension`, `MapExtension`)

### Key Patterns

- **Request-Response over TCP**: Server sends an `ActionBase` message (e.g., `RollDiceAction`, `SelectDestinationAction`), client replies with the corresponding `*Request` (e.g., `RollDiceRequest`, `SelectDestinationRequest`). The server matches by `message.Id` via `_pendingRequests`.
- **Broadcast + local WeakReferenceMessenger**: `GameRuleService.Broadcast()` sends to all remote clients AND to `WeakReferenceMessenger.Default` so the local host also processes the response.
- **Dependency Injection anti-pattern**: Some ViewModels resolve DI directly from `App.Current.Services.GetRequiredService<T>()` rather than constructor injection. This is intentional for ViewModels instantiated with `new` (e.g., page ViewModels with constructor parameters).
