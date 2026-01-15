using System;
using System.Text.Json.Serialization.Metadata;
using Antique_Tycoon.Models.Net.Tcp;

namespace Antique_Tycoon.Models;

public sealed record TcpMessageMetadata(
  TcpMessageType MessageType,
  Type ClrType,
  JsonTypeInfo JsonTypeInfo
);