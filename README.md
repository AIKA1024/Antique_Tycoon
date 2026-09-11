# 古玩大亨 · Antique_Tycoon

> 一款 Minecraft 主题的多人回合制大富翁 —— 掷骰子、走地图、买地产、下矿淘古玩、招募伙计互相使坏。

<p align="center">
  <img src="ImageAsset/%E4%B8%BB%E7%95%8C%E9%9D%A2.png" alt="主界面" width="800">
</p>

基于 **Avalonia UI + .NET 10** 的桌面端游戏，支持**局域网多人联机**（TCP 游戏协议 + UDP 房间发现）。

> ⚠️ 项目仍处于**早期开发阶段**，玩法规则、界面布局与数值平衡都在持续调整中，暂未提供正式发行版本。

---

## 目录

- [玩法概览](#玩法概览)
- [游戏内容](#游戏内容)
- [截图](#截图)
- [联机方式](#联机方式)
- [技术栈](#技术栈)
- [构建与运行](#构建与运行)
- [项目结构](#项目结构)
- [开发状态](#开发状态)

---

## 玩法概览

一局游戏由 2 名及以上玩家组成，轮流进行回合：

```
回合开始 → 投掷骰子 → 在岔路口选择前进方向 → 逐格移动
        → 触发当前节点事件（买地 / 下矿 / 招募 / 传送 …）
        → 回合结算（收租、缴税、发放薪水）
        → 下一位玩家
```

- **掷骰子**决定前进步数，地图是带分支的节点图，走到岔路口时玩家需要手动选择路线。
- 走到**无主地产**可以买下，走到**他人地产**需要缴纳过路费。
- 部分节点会触发**额外的掷骰判定**（例如「再次投骰子以获取古玩」），骰子点数直接决定收获好坏。
- 玩家资金归零即**破产**出局，最后一个活下来的人获胜。

<p align="center">
  <img src="ImageAsset/%E6%B8%B8%E7%8E%A9%E6%88%AA%E5%9B%BE.png" alt="游玩截图" width="800">
</p>

## 游戏内容

### 地图节点

地图由若干节点连接而成，不同类型的节点有不同的交互：

| 节点 | 说明 |
| --- | --- |
| 出生点 | 每回合经过可领取工资，也是部分伙计效果的触发点 |
| 地产 | 可购买、可升级；等级越高收益越高，但每回合需缴纳税收 |
| 矿洞 | 进入后可「下矿」获取古玩资源 |
| 人才市场 | 招募伙计（消耗资金换取持续生效的效果） |
| 传送门 | 传送到地图上的另一处位置 |
| 末影箱 / 末地 | 特殊节点，用于获取稀有古玩与奖励 |
| 地形 | 普通地形格，本身无交互，构成行走路径 |

### 地产经济

地产拥有**等级体系**，每一级对应一条收益修正（`+` 固定加成 或 `×` 倍率），升级需要一次性投入资金，同时每回合会产生固定**税收**压力。

<p align="center">
  <img src="ImageAsset/%E5%8D%87%E7%BA%A7%E5%9C%B0%E4%BA%A7%E7%95%8C%E9%9D%A2.png" alt="升级地产界面" width="800">
</p>

### 古玩

古玩是本作的核心资源。玩家可以在地图上**获取**、从其他玩家手中**掠夺**，然后选择时机出售变现：

- **原价出售** —— 稳妥，用于回笼资金升级地产。
- **额外收益出售** —— 收益更高，但需要承担判定风险。

<p align="center">
  <img src="ImageAsset/%E5%8B%9F%E9%9B%86%E4%BC%99%E8%AE%A1.png" alt="招募伙计" width="800">
</p>

### 伙计与效果

招募的「伙计」会在特定时机触发效果。效果本身挂载在一个统一的触发点枚举上，因此新增伙计不需要改动主流程：

| 触发点 | 时机 |
| --- | --- |
| `OnTurnStart` | 回合开始时 |
| `OnPassStartPoint` | 经过出生点（发放工资 / 额外奖励） |
| `OnPassMineCharge` | 经过矿洞缴费时 |
| `OnAppraisalRoll` | 古玩鉴宝判定（独立的掷骰） |
| `OnCalculateIncome` | 计算收入时 |
| `OnCalculateTax` | 计算税收时 |
| `OnCalculateSalary` | 计算伙计薪水时 |
| `OnBuildingUpgrade` | 建筑升级时 |

目前已实现的伙计例如：**狐狸**（提高古玩获取与掠夺的骰子点数）、**村民**、**万税爷**（加税）、**漏税王**（减税）、**蜘蛛**。

### 地图编辑器

内置**创意工坊**，可视化编辑地图：拖放节点、连接路径、配置地产的等级收益与税收、设置节点尺寸与路线优先级，编辑结果可保存为地图并在对局中使用。

<p align="center">
  <img src="ImageAsset/%E5%9C%B0%E5%9B%BE%E7%BC%96%E8%BE%91%E5%99%A8.png" alt="地图编辑器" width="800">
</p>

## 联机方式

采用**双协议**设计：

- **UDP 广播** —— 局域网内自动发现房间。
- **TCP 长连接** —— 承载游戏消息，二进制分帧（`4 字节长度 + 2 字节消息类型 + JSON 载荷`），并支持地图文件的分块传输。

房主既是玩家也是服务端。当房主自己发出请求时，会走**本地直连**优化，跳过 TCP 回环，避免不必要的序列化开销。

网络消息通过 Roslyn **增量源生成器**（`[TcpMessage]`）自动生成消息类型枚举与分发注册表，全程无反射，对 NativeAOT 友好。

## 技术栈

| 分类 | 选型 |
| --- | --- |
| UI 框架 | Avalonia 12 |
| 运行时 | .NET 10（`net10.0`，`win-x64`） |
| 架构模式 | MVVM + 依赖注入 |
| MVVM 工具 | CommunityToolkit.Mvvm、ObservableCollections |
| DI 容器 | Microsoft.Extensions.DependencyInjection |
| 消息总线 | WeakReferenceMessenger（发布 / 订阅） |
| 音效 / 音乐 | LibVLCSharp |
| 源码生成 | 自研增量生成器 `Antique_Tycoon.ProtocolGen` |
| 发布方式 | NativeAOT + `TrimMode=full` |

### 架构要点

- **页面导航**：`NavigationService` 维护历史栈，页面为 `StartPage → HallPage / CreateRoomPage → RoomPage → GamePage`，`MapEditPage` 从 `MapListPage` 进入。
- **游戏逻辑**：`GameManager` 持有全局状态（玩家、选中地图、回合、房主身份）；`GameRuleService` 驱动回合循环，为每种节点类型分发处理逻辑。
- **动作队列**：`ActionQueueService` 把异步操作串行化，保证「移动动画播完再更新坐标」这类时序正确。
- **对话框**：`DialogService` 提供带返回值 `Task<T?>` 的模态对话框栈。
- **配置持久化**：`PersistenceService` 以 JSON 形式读写 `../Configs/` 下的配置文件。

## 构建与运行

**环境要求**：Windows、.NET 10 SDK。

```bash
# Debug 运行
dotnet run --project Antique_Tycoon/Antique_Tycoon.csproj

# Release（启用 NativeAOT）
dotnet run --project Antique_Tycoon/Antique_Tycoon.csproj -c Release

# 构建整个解决方案
dotnet build Antique_Tycoon.sln
```

解决方案包含三个构建配置：

| 配置 | 说明 |
| --- | --- |
| `Debug` | 常规调试构建 |
| `Release` | 启用 `PublishAot`，产出自包含单文件程序 |
| `Debug发布` | Debug 符号 + `TRACE`，用于带调试信息发布 |

> `Release` 配置启用 NativeAOT 与完整裁剪，首次编译耗时较长属于正常现象。

## 项目结构

```
Antique_Tycoon.sln
├── Antique_Tycoon/                    # Avalonia 桌面主程序
│   ├── Models/                        # 游戏数据模型
│   │   ├── Nodes/                     # 地图节点（地产、矿洞、传送门 …）
│   │   ├── Entities/                  # 玩家、古玩、伙计
│   │   ├── Effects/                   # 伙计效果与触发上下文
│   │   └── Net/                       # TCP 请求 / 响应 / 动作消息
│   ├── Services/                      # 游戏规则、网络、导航、对话框、音效 …
│   ├── ViewModels/                    # 视图模型
│   ├── Views/                         # 界面
│   │   ├── Windows/  Widgets/  Controls/
│   │   ├── DataTemplates/             # 按数据类型自动匹配的模板
│   │   ├── Styles/  ControlThemes/
│   │   └── Behaviors/  Converters/  Extensions/
│   └── Assets/                        # 图片、音效、BGM
├── Antique_Tycoon.ProtocolGen/         # Roslyn 增量源生成器（网络消息）
└── ImageAsset/                         # 截图
```

## 开发状态

项目仍在早期阶段，当前可玩内容已包含完整回合循环、地产买卖与升级、古玩获取与出售、伙计系统、地图编辑器与局域网联机。

尚未完成 / 持续迭代的部分：数值平衡、更多伙计与古玩种类、断线重连、以及正式发行版本。

欢迎通过 Issue 反馈想法与问题。

---

## 许可

目前尚未添加开源许可证。如需在项目中复用代码，请先联系作者。
