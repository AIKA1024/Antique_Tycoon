namespace Antique_Tycoon.Models.Net.Tcp.Response.GameAction;

public class BuyEstateAction : ActionBase
{ 
  /// <summary>
  /// 处理消息前，需要等待的动画
  /// </summary>
  public string WaitAnimationToken { get; set; }

  public string EstateUuid { get; set; }

  public BuyEstateAction(string waitAnimationToken, string estateUuid)
  {
    WaitAnimationToken  = waitAnimationToken;
    EstateUuid = estateUuid;
  }
}