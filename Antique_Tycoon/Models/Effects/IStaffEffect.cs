using System.Text.Json.Serialization;
using Antique_Tycoon.Models.Effects.Contexts;
using Antique_Tycoon.Models.Effects.StaffEffectImpls;
using Antique_Tycoon.Models.Enums;

namespace Antique_Tycoon.Models.Effects;

[JsonDerivedType(typeof(CheaterEffect), "CheaterEffect")]
[JsonDerivedType(typeof(PassStartBonusEffect), "PassStartBonusEffect")]
[JsonDerivedType(typeof(MineManagerEffect), "MineManagerEffect")]
[JsonDerivedType(typeof(TaxEvasionEffect), "TaxEvasionEffect")]
[JsonDerivedType(typeof(TaxHikeEffect), "TaxHikeEffect")]

public interface IStaffEffect
{
  // 这个效果在什么时机触发？
  GameTriggerPoint TriggerPoint { get; }

  // 触发时执行的逻辑
  void Execute(GameContext context,Player owner);

  string Description { get; }
}