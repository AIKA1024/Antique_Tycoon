namespace Antique_Tycoon.Models.Enums;

public enum GameTriggerPoint
{
  /// <summary>
  /// 回合开始时
  /// </summary>
  OnTurnStart,
  /// <summary>
  /// 路过出生点/起点时 (用于发放工资、额外奖励)
  /// </summary>
  OnPassStartPoint,
  /// <summary>
  /// 路过矿洞收费时
  /// </summary>
  OnPassMineCharge,
  /// <summary>
  /// 获取古玩判定/摸金判定 (专门用于获取古玩时的投骰子，与移动无关)
  /// 此事件触发时，DiceResult 代表鉴宝的结果
  /// </summary>
  OnAppraisalRoll,
  /// <summary>
  /// 计算收入时
  /// </summary>
  OnCalculateIncome,   
  /// <summary>
  /// 计算税收时
  /// </summary>
  OnCalculateTax,
  /// <summary>
  /// 建筑升级时
  /// </summary>
  OnBuildingUpgrade,    
}