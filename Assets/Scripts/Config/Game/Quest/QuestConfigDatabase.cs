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
    }
}
