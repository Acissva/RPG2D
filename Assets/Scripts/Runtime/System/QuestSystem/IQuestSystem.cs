using QFramework;
using UnityEngine;

namespace RPG2D
{
    public interface IQuestSystem : ISystem
    {
        QuestStatus GetQuestStatus(string questId);
        bool CanAcceptQuest(string questId);
        bool AcceptQuest(string questId);
        bool AddObjectiveProgress(string questId, string objectiveId, int amount);
        bool CanCompleteQuest(string questId);
        bool CompleteQuest(string questId);
        void SetTrackedQuest(string questId);
        QuestTrackerSnapshot GetTrackedQuestSnapshot();
    }

    public class QuestSystem : AbstractSystem, IQuestSystem
    {
        private IQuestModel questModel;
        private IConfigSystem configSystem;

        protected override void OnInit()
        {
            questModel = this.GetModel<IQuestModel>();
            configSystem = this.GetSystem<IConfigSystem>();
        }

        public QuestStatus GetQuestStatus(string questId)
        {
            QuestLogEntry entry = GetQuestLogEntry(questId);
            return entry == null ? QuestStatus.NotStarted : entry.Status;
        }

        public bool CanAcceptQuest(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId))
            {
                return false;
            }

            return GetQuestConfig(questId) != null && GetQuestLogEntry(questId) == null;
        }

        public bool AcceptQuest(string questId)
        {
            if (!CanAcceptQuest(questId))
            {
                return false;
            }

            QuestConfig config = GetQuestConfig(questId);
            questModel.QuestLog[questId] = new QuestLogEntry(config);

            if (string.IsNullOrWhiteSpace(questModel.TrackedQuestId.Value))
            {
                questModel.TrackedQuestId.Value = questId;
            }

            IncreaseRevision();

            this.SendEvent(new QuestAcceptedEvent
            {
                QuestId = questId
            });

            this.SendEvent(new TrackedQuestChangedEvent
            {
                QuestId = questModel.TrackedQuestId.Value
            });

            return true;
        }

        public void SetTrackedQuest(string questId)
        {
            string newQuestId = string.IsNullOrWhiteSpace(questId) ? string.Empty : questId;

            if (!string.IsNullOrEmpty(newQuestId) && GetQuestLogEntry(newQuestId) == null)
            {
                Debug.LogWarning($"Can not track quest because it is not accepted: {newQuestId}");
                return;
            }

            if (questModel.TrackedQuestId.Value == newQuestId)
            {
                return;
            }

            questModel.TrackedQuestId.Value = newQuestId;
            IncreaseRevision();

            this.SendEvent(new TrackedQuestChangedEvent
            {
                QuestId = newQuestId
            });
        }

        public QuestTrackerSnapshot GetTrackedQuestSnapshot()
        {
            string questId = questModel.TrackedQuestId.Value;

            if (string.IsNullOrWhiteSpace(questId))
            {
                return QuestTrackerSnapshot.Empty;
            }

            QuestLogEntry entry = GetQuestLogEntry(questId);
            QuestConfig config = GetQuestConfig(questId);

            if (entry == null || config == null)
            {
                return QuestTrackerSnapshot.Empty;
            }

            QuestObjectiveConfig objectiveConfig = config.Objectives.Count > 0 ? config.Objectives[0] : null;
            QuestObjectiveProgress progress = objectiveConfig == null
                ? null
                : entry.GetObjectiveProgress(objectiveConfig.ObjectiveId);

            return new QuestTrackerSnapshot(
                true,
                questId,
                config.DisplayName,
                entry.Status,
                objectiveConfig == null ? config.Description : objectiveConfig.Description,
                progress == null ? 0 : progress.CurrentCount,
                progress == null ? 0 : progress.RequiredCount);
        }

        private QuestConfig GetQuestConfig(string questId)
        {
            return configSystem.QuestConfigDatabase == null
                ? null
                : configSystem.QuestConfigDatabase.GetQuestConfig(questId);
        }

        private QuestLogEntry GetQuestLogEntry(string questId)
        {
            if (string.IsNullOrWhiteSpace(questId))
            {
                return null;
            }

            questModel.QuestLog.TryGetValue(questId, out QuestLogEntry entry);
            return entry;
        }

        private bool AreAllObjectivesCompleted(QuestLogEntry entry)
        {
            foreach (QuestObjectiveProgress progress in entry.ObjectiveProgresses.Values)
            {
                if (!progress.IsCompleted)
                {
                    return false;
                }
            }

            return true;
        }

        private void IncreaseRevision()
        {
            questModel.QuestRevision.Value++;
        }

        public bool AddObjectiveProgress(string questId, string objectiveId, int amount)
        {
            QuestLogEntry entry = GetQuestLogEntry(questId);

            if (entry == null || entry.Status != QuestStatus.InProgress)
            {
                return false;
            }

            QuestObjectiveProgress progress = entry.GetObjectiveProgress(objectiveId);

            if (progress == null)
            {
                Debug.LogWarning($"Can not add quest objective progress. QuestId: {questId}, ObjectiveId: {objectiveId}");
                return false;
            }

            bool changed = progress.AddProgress(amount);

            if (!changed)
            {
                return false;
            }

            if (AreAllObjectivesCompleted(entry))
            {
                entry.SetStatus(QuestStatus.ReadyToTurnIn);
            }

            IncreaseRevision();

            this.SendEvent(new QuestObjectiveProgressChangedEvent
            {
                QuestId = questId,
                ObjectiveId = objectiveId,
                CurrentCount = progress.CurrentCount,
                RequiredCount = progress.RequiredCount
            });

            if (entry.Status == QuestStatus.ReadyToTurnIn)
            {
                this.SendEvent(new QuestReadyToTurnInEvent
                {
                    QuestId = questId
                });
            }

            return true;
        }

        public bool CanCompleteQuest(string questId)
        {
            QuestLogEntry entry = GetQuestLogEntry(questId);
            return entry != null && entry.Status == QuestStatus.ReadyToTurnIn;
        }

        public bool CompleteQuest(string questId)
        {
            if (!CanCompleteQuest(questId))
            {
                return false;
            }

            QuestLogEntry entry = GetQuestLogEntry(questId);
            entry.SetStatus(QuestStatus.Completed);

            IncreaseRevision();

            this.SendEvent(new QuestCompletedEvent
            {
                QuestId = questId
            });

            return true;
        }
    }
}
