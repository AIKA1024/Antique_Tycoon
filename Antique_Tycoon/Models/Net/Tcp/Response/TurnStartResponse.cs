using System.ComponentModel.DataAnnotations;

namespace Antique_Tycoon.Models.Net.Tcp.Response;

public class TurnStartResponse:ResponseBase
{
  public required Player Player { get; set; }
}