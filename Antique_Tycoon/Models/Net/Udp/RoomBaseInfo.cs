using System.IO;
using System.Net;
using System.Text.Json.Serialization;
using Avalonia.Media.Imaging;

namespace Antique_Tycoon.Models.Net;

public class RoomBaseInfo
{
  public string RoomName { get; set; } = "";
  public string Ip { get; set; } = "";
  public int Port { get; set; }
  public int CurrentPlayerCount { get; set; } = 1;
  public int MaxPlayerCount { get; set; }

  public byte[] CoverData { get; set; }
  
  [JsonIgnore]
  public Bitmap Cover
  {
    get
    {
      using var ms = new MemoryStream(CoverData);
      return new Bitmap(ms);
    }
  }

  public bool IsLanRoom { get; set; }
  // public Bitmap MapThumbnail { get; set; }
}