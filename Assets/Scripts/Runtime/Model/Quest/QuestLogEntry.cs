using System.Collections.Generic;

namespace RPG2D
{
    /// <summary>
    /// 任务日志里的一条任务记录
    /// </summary>
    /// QuestLog ：RPG 游戏中常用来表示 “任务日志 / 任务记录本”；这里的 log 与控制台的 log 不是一个含义
    /// Entry : 条目，记录；表示其中的一条记录
    /// --------------------------------------------------------------------
    /// 在编程里，Entry 经常用来表示“某个集合、日志、列表、表格中的一项”。
    /// 例如 C# 里虽然字典常叫 KeyValuePair，但很多 API 或数据结构也会把“一个键值记录”叫做 Entry。
    public class QuestLogEntry
    {
        private readonly Dictionary<string, QuestObjectiveProgress> objectiveProgresses = new();

        public string QuestId { get; }
        public QuestStatus Status { get; private set; }
        public IReadOnlyDictionary<string, QuestObjectiveProgress> ObjectiveProgresses => objectiveProgresses;

        public QuestLogEntry(QuestConfig config)
        {
            QuestId = config.QuestId;
            Status = QuestStatus.InProgress;

            for (int i = 0; i < config.Objectives.Count; i++)
            {
                QuestObjectiveConfig objective = config.Objectives[i];

                if (objective == null || string.IsNullOrWhiteSpace(objective.ObjectiveId))
                {
                    continue;
                }

                objectiveProgresses[objective.ObjectiveId] =
                    new QuestObjectiveProgress(objective.ObjectiveId, objective.RequiredCount);
            }
        }

        public QuestObjectiveProgress GetObjectiveProgress(string objectiveId)
        {
            objectiveProgresses.TryGetValue(objectiveId, out var progress);
            return progress;
        }

        public void SetStatus(QuestStatus status)
        {
            Status = status;
        }
    }
}
