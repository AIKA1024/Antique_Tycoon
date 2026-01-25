using System.Collections.Generic;
using Antique_Tycoon.Models;
using Antique_Tycoon.Models.Enums;
using Antique_Tycoon.Models.RoleBehaviors.Concrete.Sounds;
using Antique_Tycoon.Models.RoleBehaviors.Interfaces;
using LibVLCSharp.Shared;

namespace Antique_Tycoon.Services;

public class RoleStrategyFactory
{
  private readonly Dictionary<PlayerRole, ISoundStrategy> _soundStrategies = new();
  // 存储“角色类型→特殊技能策略”的映射

  // 初始化：注册所有角色的策略（新增角色时在这里加一行注册）
  public RoleStrategyFactory(LibVLC libVlc)
  {
    // 注册已有角色
    _soundStrategies.Add(PlayerRole.Steve, new SteveSound(libVlc));
    _soundStrategies.Add(PlayerRole.Sheep, new SheepSound(libVlc));
    _soundStrategies.Add(PlayerRole.Villager, new VillagerSound(libVlc));
    _soundStrategies.Add(PlayerRole.Pig, new PigSound(libVlc));
    _soundStrategies.Add(PlayerRole.Cow, new CowSound(libVlc));
    _soundStrategies.Add(PlayerRole.Creeper, new CreeperSound(libVlc));
    _soundStrategies.Add(PlayerRole.Zombie, new ZombieSound(libVlc));
  }

  // 提供获取策略的方法
  public ISoundStrategy GetSoundStrategy(PlayerRole role)
  {
    return _soundStrategies[role]; // 实际项目中可加判空逻辑
  }

}