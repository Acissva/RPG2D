using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public enum QuestObjectiveType
    {
        InteractWithObject,
    }

    //单个任务的静态配置
    [CreateAssetMenu(
        fileName = "QuestConfig",
        menuName = "RPG2D/Config/Game/Quest/Quests/Quest Config")]
    public class QuestConfig : ScriptableObject
    {
        [SerializeField] private string questId = "quest_gt_notice_report_001";
        [SerializeField] private string displayName = "小镇公告确认";
        [SerializeField, TextArea] private string description = "镇民希望你去查看公告牌，然后回来告诉他。";
        [SerializeField] private List<QuestObjectiveConfig> objectives = new();
        [SerializeField] private List<RewardItemConfig> rewards = new();

        public string QuestId => questId;
        public string DisplayName => displayName;
        public string Description => description;
        public IReadOnlyList<QuestObjectiveConfig> Objectives => objectives;
        public IReadOnlyList<RewardItemConfig> Rewards => rewards;

        private void OnValidate()
        {
            if (string.IsNullOrWhiteSpace(questId))
            {
                questId = "quest_gt_notice_report_001";
            }

            objectives ??= new List<QuestObjectiveConfig>();
            rewards ??= new List<RewardItemConfig>();

            for (int i = 0; i < objectives.Count; i++)
            {
                objectives[i]?.Validate();
            }

            for (int i = 0; i < rewards.Count; i++)
            {
                rewards[i]?.Validate();
            }
        }
    }

    //单个任务里的一个目标配置
    [Serializable]
    public class QuestObjectiveConfig
    {
        [SerializeField] private string objectiveId = "objective_read_notice_board";
        [SerializeField] private QuestObjectiveType objectiveType = QuestObjectiveType.InteractWithObject;
        [SerializeField, TextArea] private string description = "查看草地小镇公告牌";
        /// <summary>
        /// 这个任务目标关联的世界交互对象 ID
        /// </summary>
        [SerializeField] private string targetInteractionId = "notice_board_gt_001";
        [SerializeField, Min(1)] private int requiredCount = 1;

        public string ObjectiveId => objectiveId;
        public QuestObjectiveType ObjectiveType => objectiveType;
        public string Description => description;
        public string TargetInteractionId => targetInteractionId;
        public int RequiredCount => requiredCount;

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(ObjectiveId))
            {
                objectiveId = "objective_read_notice_board";
            }

            if (requiredCount < 1)
            {
                requiredCount = 1;
            }
        }
    }
}
