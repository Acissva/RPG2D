using QFramework;
using TMPro;
using UnityEngine;

namespace RPG2D
{
    public class QuestTrackerHUD : RPG2DController
    {
        [SerializeField] private GameObject contentRoot;
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI objectiveText;
        [SerializeField] private TextMeshProUGUI statusText;

        [SerializeField] private string inProgressStatusText = "进行中";
        [SerializeField] private string readyToTurnInStatusText = "可交付";
        [SerializeField] private string completedStatusText = "已完成";

        private IQuestModel questModel;
        private IQuestSystem questSystem;

        private void Awake()
        {
            questModel = this.GetModel<IQuestModel>();
            questSystem = this.GetSystem<IQuestSystem>();

            questModel.QuestRevision.RegisterWithInitValue(_ => Refresh())
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void Refresh()
        {
            QuestTrackerSnapshot snapshot = questSystem.GetTrackedQuestSnapshot();

            if (!snapshot.HasQuest)
            {
                SetVisible(false);
                return;
            }

            SetVisible(true);

            if (titleText != null)
            {
                titleText.text = $"当前任务：{snapshot.DisplayName}";
            }

            if (objectiveText != null)
            {
                objectiveText.text = GetObjectiveText(snapshot);
            }

            if (statusText != null)
            {
                statusText.text = GetStatusText(snapshot.Status);
            }
        }

        private string GetObjectiveText(QuestTrackerSnapshot snapshot)
        {
            if (snapshot.Status == QuestStatus.ReadyToTurnIn)
            {
                return "目标：返回镇民处报告";
            }

            if (snapshot.Status == QuestStatus.Completed)
            {
                return $"任务完成：{snapshot.DisplayName}";
            }

            return $"目标：{snapshot.ObjectiveDescription} {snapshot.CurrentCount}/{snapshot.RequiredCount}";
        }


        private string GetStatusText(QuestStatus status)
        {
            return status switch
            {
                QuestStatus.InProgress => inProgressStatusText,
                QuestStatus.ReadyToTurnIn => readyToTurnInStatusText,
                QuestStatus.Completed => completedStatusText,
                _ => string.Empty,
            };
        }


        private void SetVisible(bool visible)
        {
            if (contentRoot != null && contentRoot.activeSelf != visible)
            {
                contentRoot.SetActive(visible);
            }
        }
    }
}
