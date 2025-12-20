using System;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

public class DownloadMapRequest(string hash) : RequestBase
{
  public string Hash { get; set; } = hash;
}