using System.Text.Json.Serialization;
using Antique_Tycoon.Models.Configs;
using Antique_Tycoon.Models.Connections;
using Antique_Tycoon.Models.Net;
using Antique_Tycoon.Models.Net.Tcp.Request;
using Antique_Tycoon.Models.Net.Tcp.Response;
using Antique_Tycoon.Models.Net.Tcp.Response.GameAction;
using Antique_Tycoon.Models.Net.Udp;
using Antique_Tycoon.Models.Nodes;

namespace Antique_Tycoon.Models.Json;

[JsonSerializable(typeof(Player))]
[JsonSerializable(typeof(RequestBase))]
[JsonSerializable(typeof(ServiceInfo))]
[JsonSerializable(typeof(HeartbeatMessage))]
[JsonSerializable(typeof(JoinRoomResponse))]
[JsonSerializable(typeof(JoinRoomRequest))]
[JsonSerializable(typeof(UpdateRoomResponse))]
[JsonSerializable(typeof(InitGameResponse))]
[JsonSerializable(typeof(BuyEstateRequest))]
[JsonSerializable(typeof(BuyEstateAction))]
[JsonSerializable(typeof(UpdateEstateInfoResponse))]
[JsonSerializable(typeof(UpdatePlayerInfoResponse))]
[JsonSerializable(typeof(ExitRoomRequest))]
[JsonSerializable(typeof(ExitRoomResponse))]
[JsonSerializable(typeof(StartGameResponse))]
[JsonSerializable(typeof(DownloadMapRequest))]
[JsonSerializable(typeof(DownloadMapResponse))]
[JsonSerializable(typeof(TurnStartResponse))]
[JsonSerializable(typeof(RollDiceAction))]
[JsonSerializable(typeof(RollDiceRequest))]
[JsonSerializable(typeof(RollDiceResponse))]
[JsonSerializable(typeof(PlayerMoveRequest))]
[JsonSerializable(typeof(PlayerMoveResponse))]
[JsonSerializable(typeof(SelectDestinationAction))]
[JsonSerializable(typeof(SelectDestinationRequest))]
[JsonSerializable(typeof(BuyEstateResponse))]
[JsonSerializable(typeof(AntiqueChanceResponse))]
[JsonSerializable(typeof(GetAntiqueResultResponse))]
[JsonSerializable(typeof(SaleAntiqueAction))]
[JsonSerializable(typeof(SaleAntiqueRequest))]
[JsonSerializable(typeof(HireStaffAction))]
[JsonSerializable(typeof(HireStaffRequest))]
[JsonSerializable(typeof(HireStaffResponse))]
[JsonSerializable(typeof(PlunderAntiqueAction))]
[JsonSerializable(typeof(PlunderAntiqueRequest))]
[JsonSerializable(typeof(AcknowledgementResponse))]

[JsonSerializable(typeof(Map))]
[JsonSerializable(typeof(Connection))]

[JsonSerializable(typeof(PlayerConfig))]
[JsonSerializable(typeof(MainWindowConfig))]
[JsonSerializable(typeof(ServicesConfig))]
[JsonSourceGenerationOptions(WriteIndented = true)]
public partial class AppJsonContext : JsonSerializerContext;