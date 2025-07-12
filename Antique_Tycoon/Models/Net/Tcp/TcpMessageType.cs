namespace Antique_Tycoon.Models.Net.Tcp;

public enum TcpMessageType : ushort
{
  JoinRoomRequest = 1,
  JoinRoomResponse = 2,
  ChatMessage = 3
}