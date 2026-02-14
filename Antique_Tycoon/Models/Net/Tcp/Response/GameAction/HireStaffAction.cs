using System.Collections.Generic;
using Antique_Tycoon.Models.Entities;

namespace Antique_Tycoon.Models.Net.Tcp.Response.GameAction;

[TcpMessage]
public class HireStaffAction(List<IStaff> staffs):ActionBase
{
  public List<IStaff> Staffs = staffs;
}