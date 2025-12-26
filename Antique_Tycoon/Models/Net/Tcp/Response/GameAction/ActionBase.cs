using System;

namespace Antique_Tycoon.Models.Net.Tcp.Response.GameAction;

public abstract class ActionBase:ServiceRequest
{
    public ActionBase(string playerMoveResponseId)
    {
        PlayerMoveResponseId = playerMoveResponseId;
    }
}