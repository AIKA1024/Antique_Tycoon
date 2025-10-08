using System.Net.Sockets;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Antique_Tycoon.Messages;

public class ClientDisconnectedMessage(TcpClient value) : ValueChangedMessage<TcpClient>(value);