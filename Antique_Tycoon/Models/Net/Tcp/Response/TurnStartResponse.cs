namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class TurnStartResponse:ResponseBase
{
  public required string PlayerUuid { get; set; }
}