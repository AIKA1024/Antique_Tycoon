namespace Antique_Tycoon.Models.Net.Tcp;

public interface ITcpMessage
{
  public string Id { get; set; }
  public long Timestamp { get; set; }
}