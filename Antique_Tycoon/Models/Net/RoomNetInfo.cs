using System.Net;
using Avalonia.Media.Imaging;

namespace Antique_Tycoon.Models.Net;

public class RoomNetInfo
{
  public string RoomName { get; set; } = "";
  public string Ip { get; set; }
  public int Port { get; set; }
  public int CurrentPlayerCount { get; set; } = 1;
  public int MaxPlayerCount { get; set; }
  // public Bitmap MapThumbnail { get; set; }
}