using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class ServerNoticeLearningProbe : RPG2DController
    {
        private IGameServerSystem gameServerSystem;
        private IUnRegister snapshotRegistration;

        private void OnEnable()
        {
            gameServerSystem = this.GetSystem<IGameServerSystem>();
            snapshotRegistration = this.GetModel<IGameServerModel>().Snapshot.RegisterWithInitValue(OnSnapshotChanged);
        }

        private void Start()
        {
            RefreshNotice();
        }

        private void OnDisable()
        {
            snapshotRegistration?.UnRegister();
            snapshotRegistration = null;
            gameServerSystem?.CancelRequest();
        }

        [ContextMenu("Refresh Notice")]
        public void RefreshNotice()
        {
            if (!Application.isPlaying)
            {
                Debug.Log("请先进入 Play 模式，再刷新公告。");
                return;
            }

            this.SendCommand<RefreshServerNoticeCommand>();
        }

        private void OnSnapshotChanged(GameServerSnapshot snapshot)
        {
            Debug.Log($"公告状态：{snapshot.State}，提示：{snapshot.Error}，保留有效公告：{snapshot.HasNotice}");

            if (snapshot.State == GameServerRequestState.Ready)
            {
                Debug.Log(snapshot.Title + "\n" + snapshot.Content);
            }
        }
    }
}
