using QFramework;
using System.Collections;
using UnityEngine;

namespace RPG2D
{
    public class NoticePanelCloseLearningProbe : RPG2DController
    {
        private IHttpRequestHandle independentRequest;
        private INoticeSystem noticeSystem;
        private IUnRegister snapshotRegistration;
        private Coroutine observation;
        private int independentCallbacks;
        private int changesAfterClose;
        private bool independentSuccess;
        private bool ownsNotice;

        [ContextMenu("Observe Notice Panel Close")]
        public void ObservePanelClose()
        {
            if (!Application.isPlaying || !isActiveAndEnabled || observation != null)
            {
                Debug.Log("请在 Play 中启用本组件，且等待上一轮结束后再观察关窗。");
                return;
            }

            IConfigSystem config = this.GetSystem<IConfigSystem>();
            INoticeModel model = this.GetModel<INoticeModel>();
            IUISystem ui = this.GetSystem<IUISystem>();

            if (config.GameServerSettings == null || !config.GameServerSettings.TryValidate(out System.Uri root, out _) || root.AbsoluteUri != "http://127.0.0.1:5087/" || !ui.IsPanelOpen(UIPanelId.StartSceneMenu) || model.Snapshot.Value.State != NoticeRequestState.Ready || !model.Snapshot.Value.HasNotice)
            {
                Debug.LogWarning("关窗观察尚未就绪：请使用 5087 隔离宿主，保持标题菜单打开且已有 Ready 公告，再将宿主模式设为 delayed。");
                return;
            }

            observation = StartCoroutine(Observe(model, ui));
        }

        private IEnumerator Observe(INoticeModel model, IUISystem ui)
        {
            independentCallbacks = 0;
            changesAfterClose = 0;
            independentSuccess = false;
            NoticeSnapshot previous = model.Snapshot.Value;
            noticeSystem = this.GetSystem<INoticeSystem>();
            independentRequest = this.GetSystem<IHttpRequestSystem>().Get("http://127.0.0.1:5087/delay/4000/health", 8, 512, result =>
            {
                independentCallbacks++;
                independentSuccess = result.Failure == HttpRequestFailure.None && result.StatusCode == 200 && result.Body.Contains("\"service\":\"RPG2D_HTTP_LEARNING_HOST\"");
                Debug.Log($"公告关窗独立请求：Failure={result.Failure}，HTTP={result.StatusCode}，IsDone={independentRequest?.IsDone}");
            });
            ownsNotice = noticeSystem.RefreshNotice();
            yield return new WaitForSecondsRealtime(0.25f);
            bool pendingBefore = ownsNotice && !independentRequest.IsDone && model.Snapshot.Value.State == NoticeRequestState.Loading;

            if (!pendingBefore)
            {
                Debug.LogError("公告关窗汇总：FAIL，未形成两个待完成请求。请保留日志，核对宿主 delayed 模式，不将已结束请求算作取消成功。");
                ReleaseObservation();
                observation = null;
                yield break;
            }

            ui.ClosePanel(UIPanelId.StartSceneMenu);
            NoticeSnapshot closed = model.Snapshot.Value;
            bool closedWhilePending = !ui.IsPanelOpen(UIPanelId.StartSceneMenu) && closed.State == NoticeRequestState.Idle && closed.Title == previous.Title && closed.Content == previous.Content;
            ownsNotice = !closedWhilePending;
            snapshotRegistration = model.Snapshot.Register(_ => changesAfterClose++);
            Debug.Log($"公告请求中关闭：{closedWhilePending}，状态={closed.State}，保留旧公告={closed.HasNotice}，独立请求仍活动={!independentRequest.IsDone}");
            yield return new WaitForSecondsRealtime(5.5f);
            NoticeSnapshot after = model.Snapshot.Value;
            bool passed = closedWhilePending && changesAfterClose == 0 && after.State == NoticeRequestState.Idle && after.Title == previous.Title && after.Content == previous.Content && independentCallbacks == 1 && independentSuccess && independentRequest.IsDone;
            Debug.Log($"公告关窗汇总：{(passed ? "PASS" : "FAIL")}，关闭后快照变更={changesAfterClose}，独立请求回调={independentCallbacks}，独立请求成功={independentSuccess}，IsDone={independentRequest.IsDone}");
            ReleaseObservation();
            observation = null;
            ui.OpenPanel(UIPanelId.StartSceneMenu);
        }

        private void ReleaseObservation()
        {
            snapshotRegistration?.UnRegister();
            snapshotRegistration = null;
            independentRequest?.Dispose();
            independentRequest = null;

            if (ownsNotice)
            {
                noticeSystem?.CancelRequest();
            }

            ownsNotice = false;
        }

        private void OnDisable()
        {
            if (observation != null)
            {
                StopCoroutine(observation);
                observation = null;
                Debug.Log("公告关窗观察提前结束，当前不计为 PASS。");
            }

            ReleaseObservation();
        }
    }
}
