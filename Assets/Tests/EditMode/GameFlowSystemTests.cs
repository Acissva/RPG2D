using NUnit.Framework;
using QFramework;
using System;
using System.Linq;
using UnityEngine;

namespace RPG2D.Tests.EditMode
{
    public class GameFlowSystemTests
    {
        private const string CombatSourceId = "test_new_game_combat_source";

        private IArchitecture architecture;
        private ManualRuntimeUpdateSystem runtimeUpdateSystem;
        private IConfigSystem configSystem;
        private IPlayerModel playerModel;
        private IDungeonModel dungeonModel;
        private IPlayerSystem playerSystem;
        private IInventorySystem inventorySystem;
        private IQuestSystem questSystem;
        private IDungeonSystem dungeonSystem;
        private ICombatStateSystem combatStateSystem;
        private IDialogueSystem dialogueSystem;
        private ITimerSystem timerSystem;
        private IGameFlowSystem gameFlowSystem;
        private RecordingSceneSystem sceneSystem;

        private string selectedItemId;
        private string selectedQuestId;
        private int expectedMaxHP;
        private int expectedInitialHP;
        private float expectedMoveSpeed;

        [SetUp]
        public void SetUp()
        {
            architecture = GameFlowTestArchitecture.Interface;
            runtimeUpdateSystem = (ManualRuntimeUpdateSystem)architecture.GetSystem<IRuntimeUpdateSystem>();
            configSystem = architecture.GetSystem<IConfigSystem>();
            playerModel = architecture.GetModel<IPlayerModel>();
            dungeonModel = architecture.GetModel<IDungeonModel>();
            playerSystem = architecture.GetSystem<IPlayerSystem>();
            inventorySystem = architecture.GetSystem<IInventorySystem>();
            questSystem = architecture.GetSystem<IQuestSystem>();
            dungeonSystem = architecture.GetSystem<IDungeonSystem>();
            combatStateSystem = architecture.GetSystem<ICombatStateSystem>();
            dialogueSystem = architecture.GetSystem<IDialogueSystem>();
            timerSystem = architecture.GetSystem<ITimerSystem>();
            gameFlowSystem = architecture.GetSystem<IGameFlowSystem>();
            sceneSystem = (RecordingSceneSystem)architecture.GetSystem<ISceneSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            architecture.Deinit();

            architecture = null;
            runtimeUpdateSystem = null;
            configSystem = null;
            playerModel = null;
            dungeonModel = null;
            playerSystem = null;
            inventorySystem = null;
            questSystem = null;
            dungeonSystem = null;
            combatStateSystem = null;
            dialogueSystem = null;
            timerSystem = null;
            gameFlowSystem = null;
            sceneSystem = null;
            selectedItemId = null;
            selectedQuestId = null;
        }

        [Test]
        public void StartNewGame_WhenSessionContainsRuntimeState_ClearsStateBeforeLoadingMainScene()
        {
            PlayerConfig playerConfig = GetPlayerConfig();
            ItemConfig itemConfig = GetItemConfig();
            QuestConfig questConfig = GetQuestConfig();
            DungeonConfig dungeonConfig = GetDungeonConfig();

            selectedItemId = itemConfig.ItemId;
            selectedQuestId = questConfig.QuestId;
            expectedMaxHP = Mathf.Max(1, playerConfig.MaxHP);
            expectedInitialHP = Mathf.Clamp(playerConfig.InitialHP, 1, expectedMaxHP);
            expectedMoveSpeed = Mathf.Max(0f, playerConfig.MoveSpeed);

            Assert.That(playerSystem.InitializeNewGame(), Is.True);
            Assert.That(playerSystem.TakeDamage(playerModel.HP.Value), Is.True);
            Assert.That(playerModel.LifeState.Value, Is.EqualTo(PlayerLifeState.Dead));

            Assert.That(inventorySystem.AddItem(selectedItemId, 2), Is.True);
            Assert.That(questSystem.AcceptQuest(selectedQuestId), Is.True);
            questSystem.SetTrackedQuest(selectedQuestId);
            Assert.That(dungeonSystem.EnterDungeon(dungeonConfig.DungeonId), Is.True);
            combatStateSystem.ReportSourceEngaged(CombatSourceId, "Test", dungeonConfig.DungeonId);
            dialogueSystem.StartDialogue(new[] { new DialogueLine("测试角色", "旧会话对话") });

            bool sessionTimerInvoked = false;
            timerSystem.Schedule(0f, () => sessionTimerInvoked = true, TimerScope.Session);

            Assert.That(inventorySystem.GetItemCount(selectedItemId), Is.EqualTo(2));
            Assert.That(questSystem.GetTrackedQuestSnapshot().HasQuest, Is.True);
            Assert.That(dungeonModel.DungeonState, Is.EqualTo(DungeonState.Loading));
            Assert.That(combatStateSystem.IsInCombat, Is.True);
            Assert.That(dialogueSystem.IsPlaying, Is.True);

            sceneSystem.SessionCleanEvaluator = IsSessionClean;

            gameFlowSystem.StartNewGame();

            Assert.That(sceneSystem.MainGameSceneLoadCount, Is.EqualTo(1));
            Assert.That(sceneSystem.SessionWasCleanWhenMainSceneRequested, Is.True);
            Assert.That(IsSessionClean(), Is.True);

            runtimeUpdateSystem.RunUpdate();

            Assert.That(sessionTimerInvoked, Is.False);
        }

        private bool IsSessionClean()
        {
            if (playerModel.MaxHP.Value != expectedMaxHP) return false;
            if (playerModel.HP.Value != expectedInitialHP) return false;
            if (!Mathf.Approximately(playerModel.MoveSpeed.Value, expectedMoveSpeed)) return false;
            if (playerModel.LifeState.Value != PlayerLifeState.Alive) return false;
            if (inventorySystem.GetItemCount(selectedItemId) != 0) return false;
            if (inventorySystem.GetInventorySnapshot().HasAnyItem) return false;
            if (questSystem.GetQuestStatus(selectedQuestId) != QuestStatus.NotStarted) return false;
            if (questSystem.GetTrackedQuestSnapshot().HasQuest) return false;
            if (!string.IsNullOrEmpty(dungeonModel.CurrentDungeonId)) return false;
            if (dungeonModel.DungeonState != DungeonState.None) return false;
            if (dungeonModel.RoomStates.Count != 0) return false;
            if (combatStateSystem.IsInCombat) return false;
            if (combatStateSystem.GetSnapshot().ActiveSources.Count != 0) return false;
            if (dialogueSystem.IsPlaying) return false;

            return true;
        }

        private PlayerConfig GetPlayerConfig()
        {
            Assert.That(configSystem.PlayerConfig, Is.Not.Null);
            return configSystem.PlayerConfig;
        }

        private ItemConfig GetItemConfig()
        {
            Assert.That(configSystem.ItemConfigDatabase, Is.Not.Null);
            ItemConfig config = configSystem.ItemConfigDatabase.Items.FirstOrDefault(item => item != null);
            Assert.That(config, Is.Not.Null);
            return config;
        }

        private QuestConfig GetQuestConfig()
        {
            Assert.That(configSystem.QuestConfigDatabase, Is.Not.Null);
            QuestConfig config = configSystem.QuestConfigDatabase.Quests.FirstOrDefault(quest => quest != null);
            Assert.That(config, Is.Not.Null);
            return config;
        }

        private DungeonConfig GetDungeonConfig()
        {
            Assert.That(configSystem.DungeonConfigDatabase, Is.Not.Null);
            DungeonConfig config = configSystem.DungeonConfigDatabase.Dungeons.FirstOrDefault(dungeon => dungeon != null);
            Assert.That(config, Is.Not.Null);
            return config;
        }

        private sealed class GameFlowTestArchitecture : Architecture<GameFlowTestArchitecture>
        {
            protected override void Init()
            {
                this.RegisterModel<IPlayerModel>(new PlayerModel());
                this.RegisterModel<IInventoryModel>(new InventoryModel());
                this.RegisterModel<IQuestModel>(new QuestModel());
                this.RegisterModel<IDungeonModel>(new DungeonModel());
                this.RegisterModel<ICombatStateModel>(new CombatStateModel());
                this.RegisterModel<IDialogueModel>(new DialogueModel());

                this.RegisterSystem<IRuntimeUpdateSystem>(new ManualRuntimeUpdateSystem());
                this.RegisterSystem<IAssetSystem>(new AssetSystem());
                this.RegisterSystem<IConfigSystem>(new ConfigSystem());
                this.RegisterSystem<IPlayerSystem>(new PlayerSystem());
                this.RegisterSystem<IInventorySystem>(new InventorySystem());
                this.RegisterSystem<IRewardSystem>(new RewardSystem());
                this.RegisterSystem<IQuestSystem>(new QuestSystem());
                this.RegisterSystem<ITimerSystem>(new TimerSystem());
                this.RegisterSystem<ICombatStateSystem>(new CombatStateSystem());
                this.RegisterSystem<IEnemyRegistrySystem>(new EnemyRegistrySystem());
                this.RegisterSystem<ISceneSystem>(new RecordingSceneSystem());
                this.RegisterSystem<ITravelSystem>(new RecordingTravelSystem());
                this.RegisterSystem<IDungeonSystem>(new DungeonSystem());
                this.RegisterSystem<IDialogueSystem>(new DialogueSystem());
                this.RegisterSystem<IGameFlowSystem>(new GameFlowSystem());
            }
        }

        private sealed class ManualRuntimeUpdateSystem : AbstractSystem, IRuntimeUpdateSystem
        {
            private Action onUpdate;

            protected override void OnInit()
            {
            }

            public void RegisterUpdate(Action updateCallback)
            {
                onUpdate += updateCallback;
            }

            public void UnregisterUpdate(Action updateCallback)
            {
                onUpdate -= updateCallback;
            }

            public void RunUpdate()
            {
                onUpdate?.Invoke();
            }
        }

        private sealed class RecordingSceneSystem : AbstractSystem, ISceneSystem
        {
            public string CurrentSceneName => "GameFlowTestScene";
            public string LoadingSceneName => string.Empty;
            public bool IsLoading => false;
            public BindableProperty<float> LoadingProgress { get; } = new BindableProperty<float>(0f);
            public int MainGameSceneLoadCount { get; private set; }
            public bool SessionWasCleanWhenMainSceneRequested { get; private set; }
            public Func<bool> SessionCleanEvaluator { private get; set; }

            protected override void OnInit()
            {
            }

            public void LoadStartScene()
            {
            }

            public void LoadMainGameScene()
            {
                MainGameSceneLoadCount++;
                SessionWasCleanWhenMainSceneRequested = SessionCleanEvaluator != null && SessionCleanEvaluator();
            }

            public bool LoadScene(string sceneName, Guid operationId)
            {
                return true;
            }
        }

        private sealed class RecordingTravelSystem : AbstractSystem, ITravelSystem
        {
            public bool IsTraveling => false;

            protected override void OnInit()
            {
            }

            public bool Travel(TravelRequest request)
            {
                return true;
            }
        }
    }
}
