namespace Antique_Tycoon.Models;

/// <summary>
/// 提供本地玩家信息的轻量接口，避免 NetClient 直接依赖 GameManager 导致循环依赖。
/// </summary>
public interface ILocalPlayerInfo
{
    /// <summary>
    /// 本地玩家的唯一标识符。
    /// </summary>
    string Uuid { get; }
}
