using Antique_Tycoon.Models.Enums;

namespace Antique_Tycoon.Models.Net;

public class LogSegment
{
  public string Text { get; set; } = string.Empty;
  public InteractionType Type { get; set; } = InteractionType.None;
  public string? Data { get; set; } // 附加数据，比如玩家ID，坐标(10,5)，物品ID等
  public bool IsHighlight => Type != InteractionType.None;
}