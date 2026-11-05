using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class ServerNoticeLearningProbe : RPG2DController
    {
        private INoticeSystem noticeSystem;
        private IUnRegister snapshotRegistration;

        private void OnEnable()
        {
            noticeSystem = this.GetSystem<INoticeSystem>();
            snapshotRegistration = this.GetModel<INoticeModel>().Snapshot.RegisterWithInitValue(OnSnapshotChanged);
        }

        private void Start()
        {
            RefreshNotice();
        }

        private void OnDisable()
        {
            snapshotRegistration?.UnRegister();
            snapshotRegistration = null;
            noticeSystem?.CancelRequest();
        }

        [ContextMenu("Refresh Notice")]
        public void RefreshNotice()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("请先进入 Play 模式，再刷新公告。");
                return;
            }

            this.SendCommand<RefreshNoticeCommand>();
        }

        private void OnSnapshotChanged(NoticeSnapshot snapshot)
        {
            Debug.Log($"公告状态：{snapshot.State}，提示：{snapshot.Error}，保留有效公告：{snapshot.HasNotice}");

            if (snapshot.State == NoticeRequestState.Ready)
            {
                Debug.Log(snapshot.Title + "\n" + snapshot.Content);
            }
        }
    }
}
