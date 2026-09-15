using UnityEditor;
using UnityEngine;

namespace RPG2D.Editor
{
    public static class ConfigValidationTool
    {
        private static readonly string[] SearchFolders = { "Assets" };

        [MenuItem("RPG2D/Tools/Config/Validate All Configs")]
        public static void ValidateAllConfigs()
        {
            int errorCount = 0;

            ItemConfigDatabase itemDatabase = LoadSingleAsset<ItemConfigDatabase>(ref errorCount);
            DropTableConfigDatabase dropTableDatabase = LoadSingleAsset<DropTableConfigDatabase>(ref errorCount);
            QuestConfigDatabase questDatabase = LoadSingleAsset<QuestConfigDatabase>(ref errorCount);
            DungeonConfigDatabase dungeonDatabase = LoadSingleAsset<DungeonConfigDatabase>(ref errorCount);
            EnemyConfig enemyConfig = LoadSingleAsset<EnemyConfig>(ref errorCount);
            _ = LoadSingleAsset<UIPanelConfigDatabase>(ref errorCount);
            UIPanelConfig[] panelConfigs = LoadAllAssets<UIPanelConfig>(ref errorCount);

            if (itemDatabase != null && dropTableDatabase != null)
            {
                ValidateDropTableItemReferences(dropTableDatabase, itemDatabase, ref errorCount);
            }

            if (questDatabase != null && itemDatabase != null)
            {
                ValidateQuestRewardItemReferences(questDatabase, itemDatabase, ref errorCount);
            }

            if (questDatabase != null && dungeonDatabase != null)
            {
                ValidateQuestDungeonReferences(questDatabase, dungeonDatabase, ref errorCount);
            }

            if (dungeonDatabase != null && itemDatabase != null)
            {
                ValidateDungeonRewardItemReferences(dungeonDatabase, itemDatabase, ref errorCount);
            }

            if (enemyConfig != null && dropTableDatabase != null)
            {
                ValidateEnemyDropTableReference(enemyConfig, dropTableDatabase, ref errorCount);
            }

            ValidateUIPanelPrefabReferences(panelConfigs, ref errorCount);

            if (dungeonDatabase != null)
            {
                ValidateDungeonSceneReferences(dungeonDatabase, ref errorCount);
            }

            if (errorCount > 0)
            {
                Debug.LogError($"配置校验失败，共发现 {errorCount} 个问题。");
                return;
            }

            Debug.Log("配置校验通过：数据库资产与已接入的跨配置引用均有效。");
        }

        private static T LoadSingleAsset<T>(ref int errorCount) where T : ScriptableObject
        {
            string[] assetGuids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", SearchFolders);

            if (assetGuids.Length != 1)
            {
                Debug.LogError($"配置校验：预期项目中恰好存在一个 {typeof(T).Name}，实际找到 {assetGuids.Length} 个。");
                errorCount++;
                return null;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);

            if (asset != null)
            {
                return asset;
            }

            Debug.LogError($"配置校验：无法从 {assetPath} 加载 {typeof(T).Name}。");
            errorCount++;
            return null;
        }

        private static T[] LoadAllAssets<T>(ref int errorCount) where T : ScriptableObject
        {
            string[] assetGuids = AssetDatabase.FindAssets($"t:{typeof(T).Name}", SearchFolders);
            T[] assets = new T[assetGuids.Length];

            for (int assetIndex = 0; assetIndex < assetGuids.Length; assetIndex++)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[assetIndex]);
                assets[assetIndex] = AssetDatabase.LoadAssetAtPath<T>(assetPath);

                if (assets[assetIndex] != null)
                {
                    continue;
                }

                Debug.LogError($"配置校验：无法从 {assetPath} 加载 {typeof(T).Name}。");
                errorCount++;
            }

            return assets;
        }

        private static void ValidateDropTableItemReferences(DropTableConfigDatabase dropTableDatabase, ItemConfigDatabase itemDatabase, ref int errorCount)
        {
            for (int tableIndex = 0; tableIndex < dropTableDatabase.DropTables.Count; tableIndex++)
            {
                DropTableConfig dropTable = dropTableDatabase.DropTables[tableIndex];

                if (dropTable == null)
                {
                    continue;
                }

                for (int entryIndex = 0; entryIndex < dropTable.Entries.Count; entryIndex++)
                {
                    DropEntryConfig entry = dropTable.Entries[entryIndex];

                    if (entry == null)
                    {
                        Debug.LogError($"配置校验：掉落表 {dropTable.DropTableId} 的 Entries[{entryIndex}] 为空。", dropTable);
                        errorCount++;
                        continue;
                    }

                    if (itemDatabase.ContainsItem(entry.ItemId))
                    {
                        continue;
                    }

                    Debug.LogError($"配置校验：掉落表 {dropTable.DropTableId} 的 Entries[{entryIndex}] 引用了不存在的 ItemId：{entry.ItemId}。", dropTable);
                    errorCount++;
                }
            }
        }

        private static void ValidateQuestRewardItemReferences(QuestConfigDatabase questDatabase, ItemConfigDatabase itemDatabase, ref int errorCount)
        {
            for (int questIndex = 0; questIndex < questDatabase.Quests.Count; questIndex++)
            {
                QuestConfig quest = questDatabase.Quests[questIndex];

                if (quest == null)
                {
                    continue;
                }

                for (int rewardIndex = 0; rewardIndex < quest.Rewards.Count; rewardIndex++)
                {
                    RewardItemConfig reward = quest.Rewards[rewardIndex];
                    ValidateRewardItemReference(reward, $"任务 {quest.QuestId} 的 Rewards[{rewardIndex}]", quest, itemDatabase, ref errorCount);
                }
            }
        }

        private static void ValidateQuestDungeonReferences(QuestConfigDatabase questDatabase, DungeonConfigDatabase dungeonDatabase, ref int errorCount)
        {
            for (int questIndex = 0; questIndex < questDatabase.Quests.Count; questIndex++)
            {
                QuestConfig quest = questDatabase.Quests[questIndex];

                if (quest == null)
                {
                    continue;
                }

                for (int objectiveIndex = 0; objectiveIndex < quest.Objectives.Count; objectiveIndex++)
                {
                    QuestObjectiveConfig objective = quest.Objectives[objectiveIndex];

                    if (objective == null || objective.ObjectiveType != QuestObjectiveType.CompleteDungeon)
                    {
                        continue;
                    }

                    if (dungeonDatabase.ContainsDungeon(objective.TargetDungeonId))
                    {
                        continue;
                    }

                    Debug.LogError($"配置校验：任务 {quest.QuestId} 的副本目标 {objective.ObjectiveId} 引用了不存在的 DungeonId：{objective.TargetDungeonId}。", quest);
                    errorCount++;
                }
            }
        }

        private static void ValidateDungeonRewardItemReferences(DungeonConfigDatabase dungeonDatabase, ItemConfigDatabase itemDatabase, ref int errorCount)
        {
            for (int dungeonIndex = 0; dungeonIndex < dungeonDatabase.Dungeons.Count; dungeonIndex++)
            {
                DungeonConfig dungeon = dungeonDatabase.Dungeons[dungeonIndex];

                if (dungeon == null)
                {
                    continue;
                }

                for (int rewardIndex = 0; rewardIndex < dungeon.CompletionRewards.Count; rewardIndex++)
                {
                    RewardItemConfig reward = dungeon.CompletionRewards[rewardIndex];
                    ValidateRewardItemReference(reward, $"副本 {dungeon.DungeonId} 的 CompletionRewards[{rewardIndex}]", dungeon, itemDatabase, ref errorCount);
                }

                for (int roomIndex = 0; roomIndex < dungeon.Rooms.Count; roomIndex++)
                {
                    DungeonRoomConfig room = dungeon.Rooms[roomIndex];

                    if (room == null)
                    {
                        continue;
                    }

                    for (int rewardIndex = 0; rewardIndex < room.CompletionRewards.Count; rewardIndex++)
                    {
                        RewardItemConfig reward = room.CompletionRewards[rewardIndex];
                        ValidateRewardItemReference(reward, $"副本 {dungeon.DungeonId} 的房间 {room.RoomId} 的 CompletionRewards[{rewardIndex}]", dungeon, itemDatabase, ref errorCount);
                    }
                }
            }
        }

        private static void ValidateRewardItemReference(RewardItemConfig reward, string ownerDescription, Object context, ItemConfigDatabase itemDatabase, ref int errorCount)
        {
            if (reward == null)
            {
                return;
            }

            if (itemDatabase.ContainsItem(reward.ItemId))
            {
                return;
            }

            Debug.LogError($"配置校验：{ownerDescription} 引用了不存在的 ItemId：{reward.ItemId}。", context);
            errorCount++;
        }

        private static void ValidateEnemyDropTableReference(EnemyConfig enemyConfig, DropTableConfigDatabase dropTableDatabase, ref int errorCount)
        {
            if (enemyConfig.Reward == null)
            {
                return;
            }

            if (dropTableDatabase.ContainsDropTable(enemyConfig.Reward.DropTableId))
            {
                return;
            }

            Debug.LogError($"配置校验：敌人 {enemyConfig.Identity.EnemyId} 引用了不存在的 DropTableId：{enemyConfig.Reward.DropTableId}。", enemyConfig);
            errorCount++;
        }

        private static void ValidateUIPanelPrefabReferences(UIPanelConfig[] panelConfigs, ref int errorCount)
        {
            for (int configIndex = 0; configIndex < panelConfigs.Length; configIndex++)
            {
                UIPanelConfig panelConfig = panelConfigs[configIndex];

                if (panelConfig == null || string.IsNullOrWhiteSpace(panelConfig.PrefabAssetKey))
                {
                    continue;
                }

                GameObject prefab = Resources.Load<GameObject>(panelConfig.PrefabAssetKey);

                if (prefab == null)
                {
                    Debug.LogError($"配置校验：UI 面板 {panelConfig.PanelId} 引用了不存在的 PrefabAssetKey：{panelConfig.PrefabAssetKey}。", panelConfig);
                    errorCount++;
                    continue;
                }

                if (prefab.GetComponent<UIPanelController>() != null)
                {
                    continue;
                }

                Debug.LogError($"配置校验：UI 面板 {panelConfig.PanelId} 的 Prefab {panelConfig.PrefabAssetKey} 根对象缺少 UIPanelController。", panelConfig);
                errorCount++;
            }
        }

        private static void ValidateDungeonSceneReferences(DungeonConfigDatabase dungeonDatabase, ref int errorCount)
        {
            EditorBuildSettingsScene[] buildScenes = EditorBuildSettings.scenes;

            for (int dungeonIndex = 0; dungeonIndex < dungeonDatabase.Dungeons.Count; dungeonIndex++)
            {
                DungeonConfig dungeon = dungeonDatabase.Dungeons[dungeonIndex];

                if (dungeon == null)
                {
                    continue;
                }

                ValidateBuildSceneReference(dungeon.SceneName, $"副本 {dungeon.DungeonId} 的 SceneName", dungeon, buildScenes, ref errorCount);
                ValidateBuildSceneReference(dungeon.ReturnSceneName, $"副本 {dungeon.DungeonId} 的 ReturnSceneName", dungeon, buildScenes, ref errorCount);
            }
        }

        private static void ValidateBuildSceneReference(string sceneName, string fieldDescription, Object context, EditorBuildSettingsScene[] buildScenes, ref int errorCount)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                return;
            }

            for (int sceneIndex = 0; sceneIndex < buildScenes.Length; sceneIndex++)
            {
                EditorBuildSettingsScene buildScene = buildScenes[sceneIndex];

                if (!buildScene.enabled)
                {
                    continue;
                }

                SceneAsset sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(buildScene.path);

                if (sceneAsset != null && sceneAsset.name == sceneName)
                {
                    return;
                }
            }

            Debug.LogError($"配置校验：{fieldDescription} 引用了未加入或未启用的 Build Settings 场景：{sceneName}。", context);
            errorCount++;
        }
    }
}
