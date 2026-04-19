using System.IO;
using System.Text.Json.Serialization;
using Antique_Tycoon.Utilities;
using Avalonia.Media.Imaging;

namespace Antique_Tycoon.Models.Net.Udp;

public class ServiceInfo
{
  public string RoomName { get; set; } = "";
  public string Address { get; set; } = "";
  public int Port { get; set; } = App.DefaultPort;
  public int CurrentPlayerCount { get; set; } = 1;
  public int MaxPlayerCount { get; set; }
  public byte[] CoverData { get; set; } = [];
  
  public bool IsLanRoom { get; set; }
  
  [JsonIgnore]
  public Bitmap Cover
  {
    get
    {
      if (CoverData.Length == 0) return ImageHelper.GetBitmap("avares://Antique_Tycoon/Assets/Image/Loading.gif");
      
      using var ms = new MemoryStream(CoverData);
      return new Bitmap(ms);
    }
  }

}