using Antique_Tycoon.Models;
using CommunityToolkit.Mvvm.Messaging.Messages;

namespace Antique_Tycoon.Messages;

public class ChangeMapMessage:ValueChangedMessage<Map>
{
  public ChangeMapMessage(Map value) : base(value)
  {
  }
}