using Antique_Tycoon.Models.Effects.Contexts;
using Antique_Tycoon.Models.Enums;

namespace Antique_Tycoon.Models.Effects;

public interface IStaffEffect
{
  // 这个效果在什么时机触发？
  GameTriggerPoint TriggerPoint { get; }

  // 触发时执行的逻辑
  void Execute(GameContext context);

  string Description { get; }
}