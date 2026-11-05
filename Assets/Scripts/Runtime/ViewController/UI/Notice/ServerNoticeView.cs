using QFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace RPG2D
{
    [DisallowMultipleComponent]
    public sealed class ServerNoticeView : RPG2DController
    {
        [SerializeField] private TextMeshProUGUI statusText = null;
        [SerializeField] private TextMeshProUGUI noticeTitleText = null;
        [SerializeField] private TextMeshProUGUI noticeContentText = null;
        [SerializeField] private Button refreshButton = null;
        [SerializeField] private ScrollRect noticeScroll = null;

        private INoticeModel model;
        private INoticeSystem noticeSystem;
        private IUnRegister snapshotRegistration;
        private bool started;

        private void Awake()
        {
            UIPanelController panel = GetComponentInParent<UIPanelController>(true);

            if (panel == null || panel.PanelId != UIPanelId.StartSceneMenu || statusText == null || noticeTitleText == null || noticeContentText == null || refreshButton == null || noticeScroll == null || noticeScroll.content == null)
            {
                Debug.LogError($"{nameof(ServerNoticeView)} 必须位于 StartSceneMenu 内，并配置三项文本、刷新按钮和有效的 ScrollRect。", this);

                if (refreshButton != null)
                {
                    refreshButton.interactable = false;
                }

                enabled = false;
                return;
            }

            statusText.richText = false;
            noticeTitleText.richText = false;
            noticeContentText.richText = false;
            statusText.raycastTarget = false;
            noticeTitleText.raycastTarget = false;
            noticeContentText.raycastTarget = false;
            model = this.GetModel<INoticeModel>();
            noticeSystem = this.GetSystem<INoticeSystem>();
        }

        private void OnEnable()
        {
            if (noticeSystem == null)
            {
                return;
            }

            refreshButton.onClick.AddListener(OnRefreshClicked);
            snapshotRegistration = model.Snapshot.RegisterWithInitValue(Render);

            if (started)
            {
                OnRefreshClicked();
            }
        }

        private void Start()
        {
            started = true;
            OnRefreshClicked();
        }

        private void OnDisable()
        {
            snapshotRegistration?.UnRegister();
            snapshotRegistration = null;

            if (refreshButton != null)
            {
                refreshButton.onClick.RemoveListener(OnRefreshClicked);
            }

            noticeSystem?.CancelRequest();
        }

        private void OnRefreshClicked()
        {
            if (!isActiveAndEnabled || noticeSystem == null)
            {
                return;
            }

            this.SendCommand<RefreshNoticeCommand>();
        }

        private void Render(NoticeSnapshot snapshot)
        {
            statusText.text = GetStatusText(snapshot);
            refreshButton.interactable = snapshot.State != NoticeRequestState.Loading;
            string title = snapshot.HasNotice ? snapshot.Title : "服务器公告";
            string content = snapshot.HasNotice ? snapshot.Content : "暂无可用公告。";
            bool changed = noticeTitleText.text != title || noticeContentText.text != content;
            noticeTitleText.text = title;
            noticeContentText.text = content;

            if (changed)
            {
                noticeScroll.StopMovement();
                Vector2 position = noticeScroll.content.anchoredPosition;
                position.y = 0f;
                noticeScroll.content.anchoredPosition = position;
            }
        }

        private static string GetStatusText(NoticeSnapshot snapshot)
        {
            switch (snapshot.State)
            {
                case NoticeRequestState.Loading:
                    return snapshot.HasNotice ? "获取中（显示上次内容）" : "获取中…";
                case NoticeRequestState.Ready:
                    return "服务可用";
                case NoticeRequestState.Failed:
                    return "暂不可用：" + snapshot.Error + (snapshot.HasNotice ? "（显示上次内容）" : string.Empty);
                default:
                    return snapshot.HasNotice ? "尚未刷新（显示上次内容）" : "尚未连接";
            }
        }
    }
}
