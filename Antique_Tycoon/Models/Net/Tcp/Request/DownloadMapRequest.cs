using System;
using Antique_Tycoon.ProtocolGen;

namespace Antique_Tycoon.Models.Net.Tcp.Request;

[TcpMessage]
public class DownloadMapRequest(string hash) : RequestBase
{
  public string Hash { get; set; } = hash;
}