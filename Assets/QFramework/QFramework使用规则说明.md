# QFramework 使用规则说明

本文基于当前项目中 `Assets/QFramework/QFramework.cs` 的 QFramework v1.0 精简实现，以及 `Assets/Scripts` 下的实际用法整理。目标是形成一套通用规则，方便在其他 Unity 项目中复用。

当前项目使用的是 QFramework 的核心分层能力：`Architecture`、`Controller`、`System`、`Model`、`Command`、`Query`、`Utility`、事件系统和 `BindableProperty`。它不是完整插件包形态，而是把框架源码直接放在项目内，因此所有规则以当前这份源码实际提供的 API 为准。

## 1. 总体思想

QFramework 的核心目标是把 Unity 项目拆成几类职责明确的对象：

| 层级 | 主要职责 | 是否依赖 Unity 场景对象 | 常见文件 |
| --- | --- | --- | --- |
| Architecture | 应用入口和模块注册 | 不直接依赖 | `GameArchitecture.cs` |
| Controller | Unity 表现层、输入、碰撞、UI、生命周期 | 可以依赖 | `PlayerController.cs`、`UIController.cs` |
| Command | 一次业务行为、状态修改、业务流程编排 | 尽量少依赖 | `ShootCommand.cs` |
| Query | 只读查询、派生数据计算 | 不建议依赖 | `MaxBulletCountQuery.cs` |
| System | 业务系统、领域服务、持续性逻辑 | 可通过桥接 MonoBehaviour 间接依赖 | `TimeSystem.cs` |
| Model | 数据、配置、可观察状态 | 不建议依赖 | `PlayerModel.cs` |
| Utility | 无状态工具、第三方服务包装 | 不建议依赖 | `SaveUtility.cs` |

推荐的依赖方向：

```text
Controller -> Command / Query / System / Model
Command    -> System / Model / Utility / Event / Query
Query      -> System / Model / Query
System     -> System / Model / Utility / Event
Model      -> Utility / Event
Utility    -> 独立，不依赖框架层
```

禁止或尽量避免的依赖方向：

```text
Model      -> System / Command / Controller
System     -> Controller
Command    -> Controller
Query      -> Command
Utility    -> Architecture / Controller
```

通用原则：

1. View/Controller 负责“发生了什么”，Command/System/Model 负责“业务上应该怎样变化”。
2. Controller 可以读取 Model/System，但业务写操作优先通过 Command 发起。
3. Model 只保存状态和配置，不主动调用 System 或 Command。
4. System 承载跨 Model、跨规则、长期运行的业务能力。
5. Command 表示一次业务意图，例如“开枪”“换武器”“扣血”“保存关卡”。
6. Query 表示只读结果，例如“当前最大弹夹容量”“玩家当前等级加成后攻击力”。
7. Event 用于一次性通知，BindableProperty 用于持续状态绑定。
8. Unity 生命周期、物理碰撞、输入和实例化对象，优先留在 Controller 层。

## 2. 推荐目录结构

通用 Unity 项目可以采用以下结构：

```text
Assets/
  QFramework/
    QFramework.cs
  Scripts/
    GameArchitecture.cs
    Controller/
      GameController.cs
      PlayerController.cs
      UI/
    Model/
      IPlayerModel.cs
      IInventoryModel.cs
    System/
      ITimeSystem.cs
      IAudioSystem.cs
    Command/
      StartGameCommand.cs
      DamagePlayerCommand.cs
    Query/
      PlayerPowerQuery.cs
    Event/
      PlayerHpChangedEvent.cs
      CurrentWeaponChangedEvent.cs
    Utility/
      SaveUtility.cs
```

当前项目中使用了类似结构：

```text
Assets/Scripts/
  ShootingEditor2D.cs
  ViewController/
  Model/
  System/
  Command/
  Query/
  Utility/
```

对于其他项目，建议把 `ViewController` 简化为 `Controller`，或者根据团队习惯保留 `ViewController`。

## 3. 命名规则

### 3.1 Architecture 命名

推荐：

```csharp
public class GameArchitecture : Architecture<GameArchitecture>
```

也可以用项目名：

```csharp
public class ShootingEditor2D : Architecture<ShootingEditor2D>
```

通用项目中更推荐 `GameArchitecture`、`AppArchitecture` 或 `{ProjectName}Architecture`，避免和命名空间、场景名、游戏名混淆。

### 3.2 Controller 命名

推荐：

```text
PlayerController
EnemyController
UIBattleController
InventoryPanelController
```

当前项目中基础控制器命名为 `ShootingEditor2DController`，业务脚本如 `Player`、`Gun`、`UIController` 继承它。通用项目建议业务脚本也带上 `Controller` 后缀，表达它属于表现层。

### 3.3 Model 命名

接口以 `I` 开头，实现去掉 `I`：

```text
IPlayerModel -> PlayerModel
IGunConfigModel -> GunConfigModel
IInventoryModel -> InventoryModel
```

### 3.4 System 命名

接口以 `I` 开头，实现去掉 `I`：

```text
ITimeSystem -> TimeSystem
IAudioSystem -> AudioSystem
IQuestSystem -> QuestSystem
```

### 3.5 Command 命名

Command 使用动词或动宾结构：

```text
ShootCommand
ReloadCommand
PickGunCommand
DamagePlayerCommand
StartGameCommand
SaveLevelCommand
```

命名要表达业务意图，而不是表达代码动作。优先使用 `DamagePlayerCommand`，不要使用 `SetPlayerHpCommand`，除非这个命令真的只是设置数值。

### 3.6 Query 命名

Query 使用结果导向命名：

```text
MaxBulletCountQuery
PlayerPowerQuery
CanEnterDungeonQuery
InventoryItemCountQuery
```

### 3.7 Event 命名

推荐使用统一后缀：

```text
CurrentWeaponChangedEvent
PlayerDeadEvent
GamePausedEvent
LevelLoadedEvent
```

当前项目中使用了 `OnCurrentGunChanged`。迁移到其他项目时建议团队统一选择一种风格：

```text
OnCurrentGunChanged
CurrentGunChangedEvent
```

两种都可以，但不要混用。

## 4. Architecture 规则

Architecture 是整个应用的模块注册中心和框架入口。每个项目通常只需要一个主 Architecture。

### 4.1 基础模板

```csharp
using QFramework;

namespace Game
{
    public class GameArchitecture : Architecture<GameArchitecture>
    {
        protected override void Init()
        {
            this.RegisterModel<IPlayerModel>(new PlayerModel());
            this.RegisterModel<IInventoryModel>(new InventoryModel());

            this.RegisterSystem<ITimeSystem>(new TimeSystem());
            this.RegisterSystem<IAudioSystem>(new AudioSystem());

            this.RegisterUtility<ISaveUtility>(new SaveUtility());
        }
    }
}
```

当前项目示例：

```csharp
public class ShootingEditor2D : Architecture<ShootingEditor2D>
{
    protected override void Init()
    {
        this.RegisterSystem<ITimeSystem>(new TimeSystem());
        this.RegisterSystem<IStatSystem>(new StatSystem());
        this.RegisterSystem<IGunSystem>(new GunSystem());

        this.RegisterModel<IGunConfigModel>(new GunConfigModel());
        this.RegisterModel<IPlayerModel>(new PlayerModel());
    }
}
```

### 4.2 注册规则

推荐始终按接口注册：

```csharp
this.RegisterModel<IPlayerModel>(new PlayerModel());
this.RegisterSystem<ITimeSystem>(new TimeSystem());
```

对应获取时也按接口获取：

```csharp
var playerModel = this.GetModel<IPlayerModel>();
var timeSystem = this.GetSystem<ITimeSystem>();
```

不要这样混用：

```csharp
this.RegisterModel<PlayerModel>(new PlayerModel());
var model = this.GetModel<IPlayerModel>(); // 获取不到，返回 null
```

QFramework 的 `IOCContainer` 是按注册时的泛型类型作为 key 保存的。注册类型和获取类型必须一致。

### 4.3 初始化顺序

当前 QFramework 源码中，Architecture 首次访问时会执行：

1. 创建 Architecture 单例。
2. 调用 `Init()` 注册模块。
3. 执行 `OnRegisterPatch`。
4. 初始化所有 Model。
5. 初始化所有 System。
6. 标记 Architecture 已初始化。

因此即使你在 `Init()` 里先注册 System 再注册 Model，框架内部仍会先调用 Model 的 `OnInit()`，再调用 System 的 `OnInit()`。

通用建议仍然是：在 `Init()` 中按“Model -> System -> Utility”顺序书写，便于阅读。

### 4.4 首次访问和懒加载

Architecture 通过静态属性触发初始化：

```csharp
GameArchitecture.Interface
```

第一次访问时初始化，之后复用同一个实例。

在 Controller 中通常不直接保存 Architecture，而是通过基础 Controller 返回：

```csharp
IArchitecture IBelongToArchitecture.GetArchitecture()
{
    return GameArchitecture.Interface;
}
```

在测试或非 Controller 代码中可以直接使用：

```csharp
GameArchitecture.Interface.GetSystem<ITimeSystem>();
GameArchitecture.Interface.SendCommand<StartGameCommand>();
```

### 4.5 重复注册

`IOCContainer.Register<T>()` 遇到相同类型会覆盖旧实例：

```csharp
this.RegisterSystem<IAudioSystem>(new AudioSystem());
this.RegisterSystem<IAudioSystem>(new MockAudioSystem()); // 后者覆盖前者
```

这可以用于测试替换，但业务代码中要避免无意重复注册。

### 4.6 运行时注册

如果 Architecture 已经初始化，再调用 `RegisterModel` 或 `RegisterSystem`，框架会立即调用新模块的 `Init()`。

通用建议：

1. 常规模块在 `Architecture.Init()` 中一次性注册。
2. 运行时注册只用于插件、DLC、测试替换或确实动态加载的模块。
3. 动态替换系统时要考虑旧系统是否有事件订阅、MonoBehaviour 桥接对象或异步任务残留。

### 4.7 单例生命周期注意

Architecture 是静态单例。它通常会跨场景存在，直到 Unity 域重载或进程结束。

这意味着：

1. 场景切换不会自动重置 Model/System 数据。
2. 如果禁用了 Enter Play Mode 的 Domain Reload，停止再播放也可能保留静态状态。
3. 如果某个 System 在 `OnInit()` 中创建了场景内 GameObject，场景切换后这个 GameObject 可能被销毁，但 System 自身仍存在。

通用处理方式：

1. 需要跨场景持续的桥接对象，创建后调用 `Object.DontDestroyOnLoad(gameObject)`。
2. 需要每局重置的数据，提供 `ResetGameCommand`、`Reset()` 方法或专门的会话 Model。
3. 测试环境需要隔离时，可创建测试专用 Architecture，或扩展测试 reset 能力。

## 5. Controller 规则

Controller 是 Unity 世界和 QFramework 世界之间的入口。它通常是 `MonoBehaviour`，处理输入、碰撞、UI、动画、对象实例化和 Unity 生命周期。

### 5.1 基础 Controller 模板

每个项目建议定义一个基础 Controller：

```csharp
using QFramework;
using UnityEngine;

namespace Game
{
    public abstract class GameController : MonoBehaviour, IController
    {
        IArchitecture IBelongToArchitecture.GetArchitecture()
        {
            return GameArchitecture.Interface;
        }
    }
}
```

业务 Controller 继承它：

```csharp
public class PlayerController : GameController
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            this.SendCommand<PlayerJumpCommand>();
        }
    }
}
```

当前项目中对应的是：

```csharp
public abstract class ShootingEditor2DController : MonoBehaviour, IController
{
    IArchitecture IBelongToArchitecture.GetArchitecture()
    {
        return ShootingEditor2D.Interface;
    }
}
```

### 5.2 Controller 可以使用的能力

当前源码中的 `IController` 支持：

```text
GetSystem<T>()
GetModel<T>()
SendCommand<T>()
SendCommand(command)
SendQuery(query)
RegisterEvent<T>()
UnRegisterEvent<T>()
```

注意：当前 `IController` 没有实现 `ICanSendEvent`，所以 Controller 层不应该直接 `SendEvent`。如果 Controller 想通知业务变化，优先发送 Command，由 Command 或 System 再发事件。

### 5.3 Controller 的职责边界

Controller 应该做：

1. 监听 Unity 输入，例如键盘、鼠标、触摸、手柄。
2. 监听 Unity 事件，例如 `OnTriggerEnter2D`、`OnCollisionEnter2D`。
3. 控制视图对象，例如播放动画、实例化子弹、销毁场景对象。
4. 读取 Model/System 状态来刷新 UI 或表现。
5. 发送 Command 表达玩家或界面的业务意图。
6. 发送 Query 获取派生数据。
7. 注册 Event 或 BindableProperty 监听，并在销毁时取消注册。

Controller 不应该做：

1. 直接写大量业务规则。
2. 把多个 Model/System 的数据修改散落在 UI 或角色脚本里。
3. 在 Model 中存储场景对象后由 Controller 到处修改。
4. 忘记注销事件。

### 5.4 读操作与写操作

读操作可以直接拿 Model/System：

```csharp
private IPlayerModel mPlayerModel;

private void Awake()
{
    mPlayerModel = this.GetModel<IPlayerModel>();
}

private void OnGUI()
{
    GUI.Label(rect, mPlayerModel.HP.Value.ToString());
}
```

写操作优先发送 Command：

```csharp
this.SendCommand(new DamagePlayerCommand(1));
```

不要把业务写法散落在 Controller：

```csharp
// 不推荐
this.GetModel<IPlayerModel>().HP.Value -= 1;
this.GetSystem<IStatSystem>().KillCount.Value++;
SceneManager.LoadScene("GameOver");
```

更推荐：

```csharp
this.SendCommand(new DamagePlayerCommand(1));
this.SendCommand<KillEnemyCommand>();
```

### 5.5 事件注册

Controller 注册事件必须考虑生命周期：

```csharp
private void Awake()
{
    this.RegisterEvent<PlayerDeadEvent>(OnPlayerDead)
        .UnRegisterWhenGameObjectDestroy(gameObject);
}

private void OnPlayerDead(PlayerDeadEvent e)
{
    // 刷新表现
}
```

如果使用 lambda，也要绑定到 GameObject 销毁：

```csharp
this.RegisterEvent<CurrentWeaponChangedEvent>(e =>
{
    RefreshWeaponUI(e.WeaponId);
}).UnRegisterWhenGameObjectDestroy(gameObject);
```

当前项目中 UI 使用了相同方式：

```csharp
this.RegisterEvent<OnCurrentGunChanged>(e =>
{
    mMaxBulletCount = this.SendQuery(new MaxBulletCountQuery(mGunSystem.CurrentGun.Name.Value));
}).UnRegisterWhenGameObjectDestroy(gameObject);
```

### 5.6 BindableProperty 绑定

UI 绑定持续状态时，优先使用 `RegisterWithInitValue`：

```csharp
private void Awake()
{
    var playerModel = this.GetModel<IPlayerModel>();

    playerModel.HP.RegisterWithInitValue(hp =>
    {
        hpText.text = hp.ToString();
    }).UnRegisterWhenGameObjectDestroy(gameObject);
}
```

这样 UI 在注册时会立即刷新一次，不需要手动写一遍初始化 UI。

### 5.7 缓存引用

Controller 可以在 `Awake()` 或 `Start()` 中缓存 Model/System：

```csharp
private IPlayerModel mPlayerModel;
private IInventorySystem mInventorySystem;

private void Awake()
{
    mPlayerModel = this.GetModel<IPlayerModel>();
    mInventorySystem = this.GetSystem<IInventorySystem>();
}
```

注意：

1. 缓存的是 Model/System 单例，不是每帧查找。
2. `OnDestroy()` 中可以把引用置空，但更重要的是取消事件注册。
3. 不要在 Model/System 中反向缓存 Controller。

## 6. Model 规则

Model 是数据层，负责保存状态、配置和可观察数据。它不应该知道 Controller，也不应该依赖 Unity 场景对象。

### 6.1 基础模板

```csharp
using QFramework;

namespace Game
{
    public interface IPlayerModel : IModel
    {
        BindableProperty<int> HP { get; }
        BindableProperty<int> Level { get; }
    }

    public class PlayerModel : AbstractModel, IPlayerModel
    {
        public BindableProperty<int> HP { get; } = new BindableProperty<int> { Value = 100 };
        public BindableProperty<int> Level { get; } = new BindableProperty<int> { Value = 1 };

        protected override void OnInit()
        {
        }
    }
}
```

### 6.2 Model 可以使用的能力

当前源码中的 `IModel` 支持：

```text
GetUtility<T>()
SendEvent<T>()
```

Model 不支持：

```text
GetModel<T>()
GetSystem<T>()
SendCommand<T>()
SendQuery<T>()
RegisterEvent<T>()
```

这是一条非常重要的分层约束：Model 是底层数据，不依赖上层系统。

### 6.3 Model 适合放什么

适合放：

1. 玩家 HP、金币、等级、经验。
2. 背包物品列表。
3. 设置项，例如音量、画质、语言。
4. 配置表数据，例如武器配置、怪物配置、关卡配置。
5. 可保存、可恢复的游戏进度。
6. `BindableProperty<T>` 状态。

不适合放：

1. `GameObject`、`Transform`、`MonoBehaviour` 引用。
2. 输入检测。
3. UI 文本、按钮、动画。
4. 物理碰撞处理。
5. 场景跳转流程。
6. 大量跨系统业务逻辑。

### 6.4 配置 Model

当前项目中 `GunConfigModel` 是一个典型配置 Model：

```csharp
public interface IGunConfigModel : IModel
{
    GunConfigItem GetItemByName(string gunName);
}
```

通用写法：

```csharp
public interface IItemConfigModel : IModel
{
    ItemConfig GetById(int id);
    bool TryGetById(int id, out ItemConfig config);
}
```

建议：

1. 内部用字典、ScriptableObject、JSON、CSV 都可以。
2. 对外提供明确查询方法，不直接暴露可变字典。
3. 外部输入不可信时，优先提供 `TryGet`，避免字典索引器抛异常。

### 6.5 状态 Model

当前项目中 `PlayerModel` 是状态 Model：

```csharp
public interface IPlayerModel : IModel
{
    BindableProperty<int> HP { get; }
}
```

通用建议：

1. 简单状态可直接暴露 `BindableProperty<T>` 的只读属性。
2. 复杂状态建议通过方法维护不变量。
3. 不要暴露公开字段让外部随意替换 `BindableProperty` 实例。

推荐：

```csharp
public BindableProperty<int> HP { get; } = new BindableProperty<int> { Value = 100 };
```

不推荐：

```csharp
public BindableProperty<int> HP;
```

除非你非常清楚生命周期和初始化顺序。

## 7. System 规则

System 是业务系统层。它通常承载某个领域内的规则和服务，比如时间系统、背包系统、战斗系统、音频系统、任务系统。

### 7.1 基础模板

```csharp
using QFramework;

namespace Game
{
    public interface IInventorySystem : ISystem
    {
        void AddItem(int itemId, int count);
        bool HasItem(int itemId, int count);
    }

    public class InventorySystem : AbstractSystem, IInventorySystem
    {
        private IInventoryModel mInventoryModel;

        protected override void OnInit()
        {
            mInventoryModel = this.GetModel<IInventoryModel>();
        }

        public void AddItem(int itemId, int count)
        {
            mInventoryModel.AddItem(itemId, count);
            this.SendEvent(new InventoryChangedEvent());
        }

        public bool HasItem(int itemId, int count)
        {
            return mInventoryModel.GetCount(itemId) >= count;
        }
    }
}
```

### 7.2 System 可以使用的能力

当前源码中的 `ISystem` 支持：

```text
GetSystem<T>()
GetModel<T>()
GetUtility<T>()
RegisterEvent<T>()
UnRegisterEvent<T>()
SendEvent<T>()
```

System 不支持 `SendCommand<T>()`。如果一个系统需要触发另一个业务流程，优先调用其他 System 的方法，或者发事件让 Command/Controller 外层响应。也可以把编排逻辑上移到 Command。

### 7.3 System 适合放什么

适合放：

1. 时间、计时器、冷却、延迟任务。
2. 战斗计算、伤害结算、掉落规则。
3. 背包增删、任务推进、成就统计。
4. 跨多个 Model 的业务逻辑。
5. 需要长期存在的服务。
6. 领域事件发送。

不适合放：

1. 具体 UI 控件引用。
2. 场景中某个临时 GameObject 的直接引用。
3. 玩家输入轮询。
4. 大量表现层动画逻辑。

### 7.4 System 初始化

`OnInit()` 会在所有 Model 初始化之后执行。因此 System 可以安全地读取 Model：

```csharp
protected override void OnInit()
{
    var playerModel = this.GetModel<IPlayerModel>();
}
```

不要在构造函数中访问框架能力：

```csharp
public BattleSystem()
{
    // 不推荐：此时 Architecture 还没有注入
    var model = this.GetModel<IPlayerModel>();
}
```

应该放到 `OnInit()`：

```csharp
protected override void OnInit()
{
    var model = this.GetModel<IPlayerModel>();
}
```

### 7.5 MonoBehaviour 桥接

System 本身不是 `MonoBehaviour`。如果需要 `Update()`，可以创建内部桥接组件。

当前项目的 `TimeSystem` 使用了这种方式：

```csharp
public class TimeSystemUpdateBehaviour : MonoBehaviour
{
    public event Action OnUpdate;

    private void Update()
    {
        OnUpdate?.Invoke();
    }
}
```

通用模板：

```csharp
protected override void OnInit()
{
    var gameObject = new GameObject(nameof(TimeSystemUpdateBehaviour));
    Object.DontDestroyOnLoad(gameObject);

    var behaviour = gameObject.AddComponent<TimeSystemUpdateBehaviour>();
    behaviour.OnUpdate += OnUpdate;
}
```

注意：

1. 如果希望跨场景持续，调用 `DontDestroyOnLoad`。
2. 如果不希望跨场景持续，要设计场景加载后重建机制。
3. 桥接组件销毁后，System 不会自动重新 `OnInit()`。
4. System 中注册事件后，框架没有内置 `OnDispose()`，需要自己设计清理策略。

### 7.6 System 与 Command 的关系

推荐：

```text
Controller 发送 Command
Command 调用 System 方法
System 修改 Model 或发 Event
```

示例：

```csharp
public class AddItemCommand : AbstractCommand
{
    private readonly int mItemId;
    private readonly int mCount;

    public AddItemCommand(int itemId, int count)
    {
        mItemId = itemId;
        mCount = count;
    }

    protected override void OnExecute()
    {
        this.GetSystem<IInventorySystem>().AddItem(mItemId, mCount);
    }
}
```

这样 Controller 不需要知道背包内部规则。

## 8. Command 规则

Command 表示一次业务行为。它应该短小、明确、可测试。

### 8.1 基础模板

无参数命令：

```csharp
using QFramework;

namespace Game
{
    public class StartGameCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetModel<IGameModel>().State.Value = GameState.Playing;
            this.SendEvent(new GameStartedEvent());
        }
    }
}
```

有参数命令：

```csharp
public class DamagePlayerCommand : AbstractCommand
{
    private readonly int mDamage;

    public DamagePlayerCommand(int damage)
    {
        mDamage = damage;
    }

    protected override void OnExecute()
    {
        var playerModel = this.GetModel<IPlayerModel>();
        playerModel.HP.Value -= mDamage;

        if (playerModel.HP.Value <= 0)
        {
            this.SendEvent(new PlayerDeadEvent());
        }
    }
}
```

### 8.2 Command 可以使用的能力

当前源码中的 `ICommand` 支持：

```text
GetSystem<T>()
GetModel<T>()
GetUtility<T>()
SendEvent<T>()
SendCommand<T>()
SendCommand(command)
SendQuery(query)
```

Command 是业务编排能力最强的一层，但仍然不要滥用。Command 越像“一个清楚的业务动作”，项目越容易维护。

### 8.3 发送 Command

无参数命令：

```csharp
this.SendCommand<ReloadCommand>();
```

有参数命令：

```csharp
this.SendCommand(new PickItemCommand(itemId, count));
```

原因：`SendCommand<T>()` 要求命令有无参构造函数，带参数时必须传实例。

### 8.4 Command 粒度

好的粒度：

```text
ShootCommand
ReloadCommand
DamagePlayerCommand
CompleteQuestCommand
PurchaseItemCommand
```

过细的粒度：

```text
SetHpCommand
AddOneKillCountCommand
SetCurrentWeaponNameCommand
```

过粗的粒度：

```text
DoEverythingWhenPlayerPressedJCommand
```

判断标准：

1. 一个 Command 对应一个明确业务意图。
2. 命令内部可以修改多个 Model/System，但对外仍是一个动作。
3. 如果命令越来越长，说明应把领域逻辑下沉到 System。

### 8.5 高频 Command 优化

当前项目中 `ShootCommand` 使用了静态实例：

```csharp
public static readonly ShootCommand Single = new ShootCommand();
```

适用条件：

1. 命令没有每次执行不同的参数。
2. 命令内部不保存本次执行的临时可变状态。
3. 不会在多个 Architecture 之间混用。
4. 异步回调中不会依赖已经过期的命令状态。

通用建议：只有在确定这个命令非常高频且无状态时才做单例优化。普通命令直接 `new` 更清晰。

### 8.6 Command 中的延迟逻辑

当前项目中 `ShootCommand` 和 `ReloadCommand` 通过 `ITimeSystem.AddDelayTask` 安排延迟行为。

通用注意事项：

1. 延迟回调执行时，原来的状态可能已经变化，需要重新校验条件。
2. 不要在 Command 的延迟回调里直接操作可能已销毁的 GameObject。
3. 如果回调要影响表现，优先发 Event，让仍然存活的 Controller 响应。
4. 捕获 Model/System 引用通常可以，捕获 Controller 或场景对象要谨慎。

## 9. Query 规则

Query 表示只读查询。它不应该修改任何状态。

### 9.1 基础模板

```csharp
using QFramework;

namespace Game
{
    public class PlayerPowerQuery : AbstractQuery<int>
    {
        protected override int OnDo()
        {
            var playerModel = this.GetModel<IPlayerModel>();
            var equipmentSystem = this.GetSystem<IEquipmentSystem>();

            return playerModel.BasePower.Value + equipmentSystem.GetEquipmentPower();
        }
    }
}
```

带参数：

```csharp
public class ItemCountQuery : AbstractQuery<int>
{
    private readonly int mItemId;

    public ItemCountQuery(int itemId)
    {
        mItemId = itemId;
    }

    protected override int OnDo()
    {
        return this.GetModel<IInventoryModel>().GetCount(mItemId);
    }
}
```

调用：

```csharp
var count = this.SendQuery(new ItemCountQuery(itemId));
```

### 9.2 Query 可以使用的能力

当前源码中的 `IQuery<TResult>` 支持：

```text
GetModel<T>()
GetSystem<T>()
SendQuery(query)
```

Query 不支持：

```text
SendCommand<T>()
SendEvent<T>()
GetUtility<T>()
```

如果查询需要 Utility，可以让 System 或 Model 封装相关能力，或者按项目需要扩展 QFramework 接口。

### 9.3 Query 的使用场景

适合：

1. 多个字段组合出的派生值。
2. UI 需要展示但不想知道计算细节的数据。
3. 规则判断，例如是否能购买、是否能进入关卡。
4. 从配置 Model 中取加工后的结果。

不适合：

1. 修改 HP、金币、背包。
2. 触发场景跳转。
3. 发送事件。
4. 播放音效或动画。

### 9.4 Query 与 System 方法的区别

如果查询本质上属于某个领域系统，可以放在 System 方法：

```csharp
equipmentSystem.GetEquipmentPower();
```

如果查询跨多个 Model/System，或者希望 Controller 不知道细节，可以做成 Query：

```csharp
this.SendQuery(new PlayerPowerQuery());
```

通用建议：

1. 简单领域内查询：System 方法。
2. 跨层组合查询：Query。
3. UI 派生展示数据：Query。

## 10. Event 规则

Event 用于一次性通知，例如“当前武器切换了”“玩家死亡了”“关卡完成了”。

### 10.1 事件定义

推荐定义为简单数据类：

```csharp
public class CurrentWeaponChangedEvent
{
    public string WeaponId { get; set; }
}
```

或不可变结构：

```csharp
public struct PlayerDeadEvent
{
}
```

当前项目示例：

```csharp
public class OnCurrentGunChanged
{
    public string Name { get; set; }
}
```

### 10.2 发送事件

从 System、Model、Command 中发送：

```csharp
this.SendEvent(new CurrentWeaponChangedEvent
{
    WeaponId = weaponId
});
```

无数据事件：

```csharp
this.SendEvent<PlayerDeadEvent>();
```

注意：当前扩展方法 `CanSendEventExtension.SendEvent<T>(this ICanSendEvent self, T e)` 带有 `where T : new()` 约束。因此通过 `this.SendEvent(new XxxEvent())` 发送时，事件类型应提供公开无参构造函数。使用简单 class + set 属性最省心。

### 10.3 注册事件

```csharp
this.RegisterEvent<CurrentWeaponChangedEvent>(OnCurrentWeaponChanged);
```

或：

```csharp
this.RegisterEvent<CurrentWeaponChangedEvent>(e =>
{
    Refresh(e.WeaponId);
});
```

Controller 中必须绑定生命周期：

```csharp
this.RegisterEvent<CurrentWeaponChangedEvent>(e =>
{
    Refresh(e.WeaponId);
}).UnRegisterWhenGameObjectDestroy(gameObject);
```

### 10.4 手动取消注册

如果没有 GameObject 生命周期，可以保存 `IUnRegister`：

```csharp
private IUnRegister mUnRegister;

protected override void OnInit()
{
    mUnRegister = this.RegisterEvent<GamePausedEvent>(OnGamePaused);
}

private void Dispose()
{
    mUnRegister?.UnRegister();
    mUnRegister = null;
}
```

当前框架没有给 System/Model 提供统一 `Dispose()` 生命周期。如果系统会动态销毁，需要自己设计清理入口。

### 10.5 Event 与 BindableProperty 的选择

使用 Event：

1. 一次性动作。
2. 不需要保存当前值。
3. 后注册者不需要收到过去发生的事情。
4. 例如玩家死亡、切换武器、打开宝箱、任务完成。

使用 BindableProperty：

1. 持续状态。
2. UI 需要显示当前值。
3. 后注册者需要立即拿到当前值。
4. 例如 HP、金币、当前等级、当前武器名。

两者可以一起使用：

```text
CurrentWeaponName 是 BindableProperty
CurrentWeaponChangedEvent 用来通知需要重新计算派生数据
```

## 11. BindableProperty 规则

`BindableProperty<T>` 是 QFramework 中用于数据绑定的可观察属性。

### 11.1 基础用法

定义：

```csharp
public BindableProperty<int> HP { get; } = new BindableProperty<int> { Value = 100 };
```

读取：

```csharp
var hp = playerModel.HP.Value;
```

修改：

```csharp
playerModel.HP.Value -= 10;
```

注册监听：

```csharp
playerModel.HP.Register(hp =>
{
    Debug.Log(hp);
});
```

注册并立即获得当前值：

```csharp
playerModel.HP.RegisterWithInitValue(hp =>
{
    hpText.text = hp.ToString();
}).UnRegisterWhenGameObjectDestroy(gameObject);
```

### 11.2 触发规则

`BindableProperty<T>.Value` 只有在新旧值不相等时触发回调：

```csharp
if (value == null && mValue == null) return;
if (value != null && value.Equals(mValue)) return;
```

因此：

1. 赋相同值不会触发。
2. 对引用类型，修改对象内部字段不会触发，除非重新赋值。
3. 自定义类型的相等性由 `Equals()` 决定。
4. 浮点数频繁变化会频繁触发，必要时在业务层做阈值判断。

### 11.3 不要共享 BindableProperty 实例

当前项目在 `GunSystem` 中特别指出了这个坑：

```csharp
Name = new BindableProperty<string> { Value = CurrentGun.Name.Value }
```

不要这样：

```csharp
Name = CurrentGun.Name; // 不推荐：两个对象共享同一个 BindableProperty
```

否则两个业务对象会绑定到同一个属性实例，修改一个会影响另一个。

### 11.4 暴露方式

推荐：

```csharp
public BindableProperty<int> Coin { get; } = new BindableProperty<int> { Value = 0 };
```

不推荐：

```csharp
public BindableProperty<int> Coin = new BindableProperty<int>();
```

原因：

1. `get; }` 可以防止外部替换整个属性对象。
2. 外部只能改 `.Value`，绑定关系不会丢。

### 11.5 空值注意

当前实现的 `ToString()` 是：

```csharp
public override string ToString()
{
    return Value.ToString();
}
```

如果 `Value` 为 `null`，会抛出异常。因此 UI 展示时更安全：

```csharp
var text = property.Value?.ToString() ?? string.Empty;
```

## 12. Utility 规则

Utility 用于放无状态工具或第三方服务包装。

### 12.1 接口式 Utility

如果需要通过框架注入，定义接口：

```csharp
public interface ISaveUtility : IUtility
{
    void Save(string key, string json);
    string Load(string key);
}

public class PlayerPrefsSaveUtility : ISaveUtility
{
    public void Save(string key, string json)
    {
        PlayerPrefs.SetString(key, json);
    }

    public string Load(string key)
    {
        return PlayerPrefs.GetString(key, string.Empty);
    }
}
```

注册：

```csharp
this.RegisterUtility<ISaveUtility>(new PlayerPrefsSaveUtility());
```

使用：

```csharp
var saveUtility = this.GetUtility<ISaveUtility>();
```

### 12.2 静态工具类

当前项目中的 `RectHelper` 是静态工具类，没有注册进 QFramework：

```csharp
public static class RectHelper
{
    public static Rect RectForAnchorCenter(float x, float y, float width, float height)
    {
        ...
    }
}
```

通用判断：

1. 纯数学、纯字符串、纯 Rect 计算，可以用静态工具类。
2. 需要替换实现、测试 mock、访问平台 API，建议注册为 Utility。
3. Utility 不应该保存复杂业务状态；状态应该放 Model/System。

## 13. 通信规则

### 13.1 Controller 到业务层

玩家输入、UI 点击、碰撞等，通过 Command 表达意图：

```csharp
private void OnTriggerEnter2D(Collider2D other)
{
    if (other.CompareTag("Player"))
    {
        this.SendCommand(new PickItemCommand(itemId, count));
    }
}
```

### 13.2 业务层到 Controller

业务层不要直接拿 Controller。通过 Event 或 BindableProperty 通知：

```csharp
this.SendEvent(new PlayerDeadEvent());
```

Controller 响应：

```csharp
this.RegisterEvent<PlayerDeadEvent>(e =>
{
    gameOverPanel.SetActive(true);
}).UnRegisterWhenGameObjectDestroy(gameObject);
```

### 13.3 Command 与 System

Command 负责编排，System 负责领域能力：

```csharp
protected override void OnExecute()
{
    this.GetSystem<IInventorySystem>().AddItem(mItemId, mCount);
}
```

### 13.4 System 与 Model

System 可以读取和修改 Model：

```csharp
var model = this.GetModel<IInventoryModel>();
model.AddItem(itemId, count);
```

### 13.5 Model 发事件

Model 可以在数据变化时发事件，但要谨慎。

适合：

```csharp
public void SetLanguage(string language)
{
    Language.Value = language;
    this.SendEvent(new LanguageChangedEvent { Language = language });
}
```

不适合：

```text
Model 里根据事件调用 System、跳转场景、播放 UI 动画
```

如果逻辑复杂，放到 System 或 Command。

## 14. 新功能接入流程

新增一个功能时，可以按以下顺序思考。

### 14.1 明确功能类型

先判断它主要属于哪类：

```text
只是表现和输入 -> Controller
需要保存的数据 -> Model
一组长期业务能力 -> System
一次业务动作 -> Command
只读派生结果 -> Query
一次性通知 -> Event
无状态工具服务 -> Utility
```

### 14.2 建 Model

如果功能需要状态，先建 Model：

```csharp
public interface IScoreModel : IModel
{
    BindableProperty<int> Score { get; }
}

public class ScoreModel : AbstractModel, IScoreModel
{
    public BindableProperty<int> Score { get; } = new BindableProperty<int> { Value = 0 };

    protected override void OnInit()
    {
    }
}
```

### 14.3 建 System

如果功能有规则，建 System：

```csharp
public interface IScoreSystem : ISystem
{
    void AddScore(int score);
}

public class ScoreSystem : AbstractSystem, IScoreSystem
{
    private IScoreModel mScoreModel;

    protected override void OnInit()
    {
        mScoreModel = this.GetModel<IScoreModel>();
    }

    public void AddScore(int score)
    {
        mScoreModel.Score.Value += score;
        this.SendEvent(new ScoreChangedEvent { Score = mScoreModel.Score.Value });
    }
}
```

### 14.4 注册模块

```csharp
protected override void Init()
{
    this.RegisterModel<IScoreModel>(new ScoreModel());
    this.RegisterSystem<IScoreSystem>(new ScoreSystem());
}
```

### 14.5 建 Command

```csharp
public class AddScoreCommand : AbstractCommand
{
    private readonly int mScore;

    public AddScoreCommand(int score)
    {
        mScore = score;
    }

    protected override void OnExecute()
    {
        this.GetSystem<IScoreSystem>().AddScore(mScore);
    }
}
```

### 14.6 Controller 发送 Command

```csharp
public class EnemyController : GameController
{
    private void OnDead()
    {
        this.SendCommand(new AddScoreCommand(100));
    }
}
```

### 14.7 UI 绑定状态

```csharp
public class ScoreUIController : GameController
{
    [SerializeField] private Text scoreText;

    private void Awake()
    {
        this.GetModel<IScoreModel>().Score
            .RegisterWithInitValue(score =>
            {
                scoreText.text = score.ToString();
            })
            .UnRegisterWhenGameObjectDestroy(gameObject);
    }
}
```

## 15. 当前项目实践提炼

当前项目提供了几个可复用模式。

### 15.1 基础 Controller 模式

所有需要访问 QFramework 的 MonoBehaviour 继承项目基础 Controller：

```csharp
public abstract class ShootingEditor2DController : MonoBehaviour, IController
{
    IArchitecture IBelongToArchitecture.GetArchitecture()
    {
        return ShootingEditor2D.Interface;
    }
}
```

迁移到其他项目时只需要替换 Architecture 类型。

### 15.2 Controller 发送命令

碰撞拾取：

```csharp
this.SendCommand(new PickGunCommand(Name, BulletCountInGun, BulletCountOutGun));
```

按键换武器：

```csharp
this.SendCommand<ShiftGunCommand>();
```

这是推荐模式：Controller 不直接改业务数据，而是表达意图。

### 15.3 Command 编排业务

`ReloadCommand` 会：

1. 获取当前枪状态。
2. 获取配置 Model。
3. 计算需要装填的子弹。
4. 修改枪状态为 Reload。
5. 通过 TimeSystem 延迟恢复并结算弹药。

这说明 Command 可以承载一次完整业务流程，但如果流程继续变复杂，应把领域规则下沉到 `IGunSystem`。

### 15.4 System 管理领域状态

`IGunSystem` 负责：

1. 当前武器。
2. 武器队列。
3. 捡枪规则。
4. 切枪规则。
5. 切换后发送事件。

这比把所有逻辑写在 `Gun` Controller 中更容易复用和测试。

### 15.5 Query 隔离派生数据

`MaxBulletCountQuery` 根据枪名查询最大弹夹容量。UI 和 Gun Controller 不直接访问配置字典，而是发送 Query：

```csharp
mMaxBulletCount = this.SendQuery(new MaxBulletCountQuery(mGunInfo.Name.Value));
```

通用价值：UI 不需要知道配置存在哪里、怎样计算。

### 15.6 事件驱动刷新派生数据

当前枪改变时，System 发送事件：

```csharp
this.SendEvent(new OnCurrentGunChanged
{
    Name = nextGunName
});
```

UI 收到事件后重新查询最大弹夹容量。这是“事件 + 查询”的良好组合。

### 15.7 不是所有脚本都必须接入 QFramework

当前项目中 `CameraController`、`Enemy`、`Trigger2DCheck`、`LevelEditor`、`LevelPlayer` 等脚本没有接入 QFramework。它们主要是局部表现、编辑器工具或简单组件逻辑。

通用规则：

1. 只影响自身 GameObject 的简单表现脚本，可以不接入 QFramework。
2. 不涉及全局业务状态的工具脚本，可以不接入 QFramework。
3. 涉及跨对象、跨场景、可保存状态、UI 绑定、业务规则时，再接入 QFramework。

不要为了“统一”把所有 MonoBehaviour 都继承基础 Controller。框架应该降低复杂度，而不是增加样板代码。

## 16. 常见坑与规避方式

### 16.1 忘记注册模块

问题：

```csharp
var system = this.GetSystem<IAudioSystem>(); // null
```

原因：没有在 Architecture 中注册。

解决：

```csharp
this.RegisterSystem<IAudioSystem>(new AudioSystem());
```

### 16.2 注册类型和获取类型不一致

问题：

```csharp
this.RegisterSystem<AudioSystem>(new AudioSystem());
this.GetSystem<IAudioSystem>(); // null
```

解决：统一按接口注册和获取。

### 16.3 在构造函数中使用框架 API

问题：

```csharp
public TimeSystem()
{
    this.GetModel<IGameModel>(); // Architecture 尚未注入
}
```

解决：放到 `OnInit()`。

### 16.4 Controller 事件未注销

问题：

```csharp
this.RegisterEvent<GameOverEvent>(e => ShowGameOver());
```

如果 Controller 销毁后事件仍存在，可能回调到已销毁对象。

解决：

```csharp
this.RegisterEvent<GameOverEvent>(e => ShowGameOver())
    .UnRegisterWhenGameObjectDestroy(gameObject);
```

### 16.5 BindableProperty 共享实例

问题：

```csharp
newGun.Name = currentGun.Name;
```

两个对象共用同一份可绑定属性。

解决：

```csharp
newGun.Name = new BindableProperty<string> { Value = currentGun.Name.Value };
```

### 16.6 误以为 Event 会保存状态

Event 只通知当下发生的事情。晚注册的监听者不会收到过去的事件。

需要当前值时，使用 Model 的 `BindableProperty` 或 Query。

### 16.7 Query 中修改数据

不推荐：

```csharp
protected override int OnDo()
{
    this.GetModel<IPlayerModel>().HP.Value -= 1;
    return 0;
}
```

Query 必须只读。修改状态请使用 Command 或 System。

### 16.8 Command 太大

如果一个 Command 超过几十行，并且包含多个领域的细节，考虑拆分：

1. 把领域规则移动到 System。
2. 把只读计算移动到 Query。
3. 把多个业务意图拆成多个 Command。

### 16.9 System 持有场景对象

System 是 Architecture 单例的一部分，生命周期可能长于场景。直接持有场景对象容易在切场景后失效。

更推荐：

1. Controller 持有场景对象。
2. System 持有纯数据、ID、配置。
3. System 发 Event，Controller 响应并操作场景对象。

### 16.10 场景切换后 System 的 MonoBehaviour 桥接失效

如果 System 创建了 GameObject 用于 Update，它可能在场景切换时被销毁。

解决：

```csharp
Object.DontDestroyOnLoad(updateBehaviourGameObj);
```

或者监听场景加载并重建桥接对象。

### 16.11 `SendCommand<T>()` 不能用于有参构造

无参：

```csharp
this.SendCommand<ReloadCommand>();
```

有参：

```csharp
this.SendCommand(new DamagePlayerCommand(1));
```

### 16.12 事件类型必须精确匹配

注册：

```csharp
this.RegisterEvent<BaseEvent>(OnBaseEvent);
```

发送：

```csharp
this.SendEvent(new DerivedEvent());
```

不会触发 `BaseEvent` 的监听。当前 `TypeEventSystem` 按具体 `typeof(T)` 分发，不做继承链匹配。

### 16.13 `GetModel` / `GetSystem` 返回 null

当前框架获取失败时返回 `null`，不会主动抛错。建议在重要模块初始化时主动检查：

```csharp
protected override void OnInit()
{
    mPlayerModel = this.GetModel<IPlayerModel>();
    Debug.Assert(mPlayerModel != null, "IPlayerModel is not registered.");
}
```

## 17. 测试建议

### 17.1 测试 Command

Command 适合测试：

```csharp
GameArchitecture.Interface.SendCommand(new DamagePlayerCommand(10));
var hp = GameArchitecture.Interface.GetModel<IPlayerModel>().HP.Value;
Assert.AreEqual(90, hp);
```

注意 Architecture 是静态单例，测试之间可能共享状态。需要隔离时：

1. 使用测试专用 Architecture。
2. 提供 Reset 命令重置 Model/System。
3. 用 `OnRegisterPatch` 在首次初始化前替换模块。

### 17.2 测试 Query

Query 应该稳定、无副作用：

```csharp
var power = GameArchitecture.Interface.SendQuery(new PlayerPowerQuery());
Assert.AreEqual(expectedPower, power);
```

### 17.3 测试 System

System 可通过接口直接调用：

```csharp
var inventorySystem = GameArchitecture.Interface.GetSystem<IInventorySystem>();
inventorySystem.AddItem(1001, 3);
```

如果 System 依赖 Unity `Update()`，使用 Unity Test Framework 的 PlayMode 测试更合适。

### 17.4 测试事件

```csharp
var received = false;

var unregister = GameArchitecture.Interface.RegisterEvent<PlayerDeadEvent>(e =>
{
    received = true;
});

GameArchitecture.Interface.SendCommand(new DamagePlayerCommand(999));

Assert.IsTrue(received);
unregister.UnRegister();
```

## 18. 代码模板汇总

### 18.1 Architecture

```csharp
public class GameArchitecture : Architecture<GameArchitecture>
{
    protected override void Init()
    {
        this.RegisterModel<IPlayerModel>(new PlayerModel());
        this.RegisterSystem<ITimeSystem>(new TimeSystem());
        this.RegisterUtility<ISaveUtility>(new SaveUtility());
    }
}
```

### 18.2 Base Controller

```csharp
public abstract class GameController : MonoBehaviour, IController
{
    IArchitecture IBelongToArchitecture.GetArchitecture()
    {
        return GameArchitecture.Interface;
    }
}
```

### 18.3 Model

```csharp
public interface IPlayerModel : IModel
{
    BindableProperty<int> HP { get; }
}

public class PlayerModel : AbstractModel, IPlayerModel
{
    public BindableProperty<int> HP { get; } = new BindableProperty<int> { Value = 100 };

    protected override void OnInit()
    {
    }
}
```

### 18.4 System

```csharp
public interface IPlayerSystem : ISystem
{
    void Damage(int damage);
}

public class PlayerSystem : AbstractSystem, IPlayerSystem
{
    private IPlayerModel mPlayerModel;

    protected override void OnInit()
    {
        mPlayerModel = this.GetModel<IPlayerModel>();
    }

    public void Damage(int damage)
    {
        mPlayerModel.HP.Value -= damage;

        if (mPlayerModel.HP.Value <= 0)
        {
            this.SendEvent(new PlayerDeadEvent());
        }
    }
}
```

### 18.5 Command

```csharp
public class DamagePlayerCommand : AbstractCommand
{
    private readonly int mDamage;

    public DamagePlayerCommand(int damage)
    {
        mDamage = damage;
    }

    protected override void OnExecute()
    {
        this.GetSystem<IPlayerSystem>().Damage(mDamage);
    }
}
```

### 18.6 Query

```csharp
public class PlayerHpPercentQuery : AbstractQuery<float>
{
    protected override float OnDo()
    {
        var playerModel = this.GetModel<IPlayerModel>();
        return playerModel.HP.Value / 100f;
    }
}
```

### 18.7 Event

```csharp
public class PlayerDeadEvent
{
}
```

### 18.8 Controller 使用

```csharp
public class TrapController : GameController
{
    [SerializeField] private int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            this.SendCommand(new DamagePlayerCommand(damage));
        }
    }
}
```

### 18.9 UI 绑定

```csharp
public class PlayerHpUIController : GameController
{
    [SerializeField] private Text hpText;

    private void Awake()
    {
        this.GetModel<IPlayerModel>().HP
            .RegisterWithInitValue(hp =>
            {
                hpText.text = hp.ToString();
            })
            .UnRegisterWhenGameObjectDestroy(gameObject);
    }
}
```

## 19. 团队约定建议

在其他项目中落地时，建议团队提前约定以下事项：

1. Architecture 命名：`GameArchitecture` 还是 `{ProjectName}Architecture`。
2. Event 命名：`XxxEvent` 还是 `OnXxx`。
3. Controller 是否统一带 `Controller` 后缀。
4. 是否允许 Controller 直接修改 Model 的 `BindableProperty.Value`。
5. System 是否允许直接调用 Unity API。
6. 场景切换时哪些 Model 需要重置，哪些需要保留。
7. 是否为 System/Model 扩展 Dispose 或 Reset 生命周期。
8. 配置数据放 Model、Utility 还是 ScriptableObject。
9. 测试是否使用专门的 TestArchitecture。
10. 高频 Command 是否允许单例化。

推荐默认约定：

1. Controller 只读 Model/System，写操作走 Command。
2. Command 可修改 Model，也可调用 System；复杂领域逻辑放 System。
3. Query 永远只读。
4. Model 不依赖 System/Command/Controller。
5. System 不持有场景对象。
6. Event 必须注销，UI 状态优先用 BindableProperty。
7. 所有模块按接口注册和获取。

## 20. 快速检查清单

提交或评审 QFramework 相关代码时，可以检查：

```text
[ ] 新 Model/System/Utility 是否已在 Architecture 注册
[ ] 注册类型和获取类型是否一致
[ ] Controller 是否继承了项目基础 Controller
[ ] Controller 写业务状态是否通过 Command
[ ] Command 是否表达明确业务意图
[ ] Query 是否没有副作用
[ ] Model 是否没有引用 Controller 或场景对象
[ ] System 是否没有长期持有会被销毁的场景对象
[ ] Event 注册是否会注销
[ ] BindableProperty 是否没有被多个对象共享实例
[ ] 场景切换后需要保留或重置的数据是否明确
[ ] 高频命令单例是否无状态
[ ] 异步/延迟回调是否重新校验状态
[ ] UI 是否使用 RegisterWithInitValue 初始化显示
```

## 21. 一句话总结

QFramework 的好用之处不在于把所有脚本都纳入框架，而在于给“状态、规则、行为、查询、通知、表现”划清边界：

```text
Controller 接收 Unity 世界的输入
Command 表达一次业务动作
System 执行业务规则
Model 保存业务状态
Query 提供只读结果
Event 通知一次性变化
BindableProperty 绑定持续状态
```

只要保持这些边界稳定，项目规模扩大后仍然能比较自然地扩展、测试和维护。
