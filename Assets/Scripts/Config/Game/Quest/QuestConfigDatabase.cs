using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    //全部任务的目录 / 索引
    [CreateAssetMenu(
        fileName = "QuestConfigDatabase",
        menuName = "RPG2D/Config/Game/Quest/Quest Config Database")]
    public class QuestConfigDatabase : ScriptableObject
    {
        [SerializeField] private List<QuestConfig> quests = new();

        public IReadOnlyList<QuestConfig> Quests => quests;

        public QuestConfig GetQuestConfig(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId))
            {
                return null;
            }

            for (int i = 0; i < quests.Count; i++)
            {
                QuestConfig quest = quests[i];

                if (quest != null && quest.QuestId == questId)
                {
                    return quest;
                }
            }

            return null;
        }

        private void OnValidate()
        {
            quests ??= new List<QuestConfig>();

            HashSet<string> questIds = new();

            for (int i = 0; i < quests.Count; i++)
            {
                QuestConfig quest = quests[i];

                if (quest == null)
                {
                    Debug.LogError($"{nameof(QuestConfigDatabase)} 的 Quests[{i}] 为空。", this);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(quest.QuestId))
                {
                    Debug.LogError($"{nameof(QuestConfigDatabase)} 中存在 QuestId 为空的配置，索引={i}。", quest);
                    continue;
                }

                if (!questIds.Add(quest.QuestId))
                {
                    Debug.LogError($"{nameof(QuestConfigDatabase)} 中存在重复 QuestId：{quest.QuestId}。", quest);
                }
            }
        }
    }
}
