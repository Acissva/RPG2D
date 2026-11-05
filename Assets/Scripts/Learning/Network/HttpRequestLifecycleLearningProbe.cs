using QFramework;
using System;
using System.Collections;
using UnityEngine;

namespace RPG2D
{
    public class HttpRequestLifecycleLearningProbe : MonoBehaviour
    {
        private const string Root = "http://127.0.0.1:5087";
        private IArchitecture learningArchitecture;
        private LearningUpdateSystem framePump;
        private IHttpRequestSystem http;
        private IHttpRequestHandle requestA;
        private IHttpRequestHandle requestB;
        private Coroutine routine;
        private int callbacks;
        private bool quitArmed;
        private bool quitReady;

        [ContextMenu("Run HTTP Deinit Probe")]
        public void RunDeinitProbe()
        {
            if (!CanStart())
            {
                return;
            }

            routine = StartCoroutine(ObserveDeinit());
        }

        [ContextMenu("Arm HTTP Quit Probe")]
        public void ArmQuitProbe()
        {
            if (!CanStart())
            {
                return;
            }

            BeginPair();
            quitArmed = true;
            Application.quitting += AfterHttpShutdown;
            routine = StartCoroutine(WaitForQuitReady());
        }

        private bool CanStart()
        {
            if (!Application.isPlaying || !isActiveAndEnabled)
            {
                Debug.Log("请进入 Play 模式并启用本组件，再开始生命周期观察。");
                return false;
            }

            if (learningArchitecture != null || routine != null)
            {
                Debug.Log("当前观察尚未结束，请先等待汇总或完成退出观察。");
                return false;
            }

            return true;
        }

        private void BeginPair()
        {
            callbacks = 0;
            learningArchitecture = LearningArchitecture.Interface;
            framePump = (LearningUpdateSystem)learningArchitecture.GetSystem<IRuntimeUpdateSystem>();
            http = learningArchitecture.GetSystem<IHttpRequestSystem>();
            requestA = http.Get(Root + "/delay/5000/health", 10, 256, OnUnexpectedResult);
            requestB = http.PostJson(Root + "/delay/5000/echo", "{\"request\":\"lifecycle\"}", 10, 256, OnUnexpectedResult);
        }

        private void Update()
        {
            framePump?.Tick();
        }

        private bool BothPending()
        {
            return requestA != null && requestB != null && !requestA.IsDone && !requestB.IsDone && callbacks == 0;
        }

        private IEnumerator ObserveDeinit()
        {
            BeginPair();
            yield return new WaitForSecondsRealtime(0.25f);
            bool pendingBefore = BothPending() && framePump.ListenerCount == 1;
            learningArchitecture.Deinit();
            learningArchitecture = null;
            http.Deinit();
            bool rejected = RejectsNewRequest();
            yield return new WaitForSecondsRealtime(0.25f);
            bool passed = pendingBefore && requestA.IsDone && requestB.IsDone && callbacks == 0 && framePump.ListenerCount == 0 && rejected;
            Debug.Log($"HTTP Deinit 汇总：{(passed ? "PASS" : "FAIL")}，之前均活动={pendingBefore}，A.IsDone={requestA.IsDone}，B.IsDone={requestB.IsDone}，回调={callbacks}，Update监听={framePump.ListenerCount}，拒绝新请求={rejected}");
            ReleaseObservation();
            routine = null;
        }

        private IEnumerator WaitForQuitReady()
        {
            yield return new WaitForSecondsRealtime(0.25f);
            quitReady = BothPending() && framePump.ListenerCount == 1;
            Debug.Log($"HTTP Quit 已就绪：{quitReady}；请现在停止 Play，勿等待 5 秒响应完成。");
            routine = null;
        }

        private void AfterHttpShutdown()
        {
            bool rejected = RejectsNewRequest();
            bool passed = quitArmed && quitReady && requestA.IsDone && requestB.IsDone && callbacks == 0 && framePump.ListenerCount == 0 && rejected;
            Debug.Log($"HTTP Quit 汇总：{(passed ? "PASS" : "FAIL")}，A.IsDone={requestA.IsDone}，B.IsDone={requestB.IsDone}，回调={callbacks}，Update监听={framePump.ListenerCount}，拒绝新请求={rejected}");
            Application.quitting -= AfterHttpShutdown;
            ReleaseObservation();
        }

        private bool RejectsNewRequest()
        {
            try
            {
                IHttpRequestHandle unexpected = http.Get(Root + "/health", 1, 256, OnUnexpectedResult);
                unexpected.Dispose();
                return false;
            }
            catch (InvalidOperationException)
            {
                return true;
            }
        }

        private void OnUnexpectedResult(HttpRequestResult result)
        {
            callbacks++;
            Debug.Log($"HTTP 生命周期请求已交付：Failure={result.Failure}，HTTP={result.StatusCode}；本轮应在响应完成前收尾。");
        }

        private void ReleaseObservation()
        {
            Application.quitting -= AfterHttpShutdown;

            if (learningArchitecture != null)
            {
                learningArchitecture.Deinit();
                learningArchitecture = null;
            }

            requestA?.Dispose();
            requestB?.Dispose();
            requestA = null;
            requestB = null;
            http = null;
            framePump = null;
            quitArmed = false;
            quitReady = false;
        }

        private void OnDisable()
        {
            if (routine != null)
            {
                StopCoroutine(routine);
                routine = null;
            }

            if (quitArmed)
            {
                Debug.Log("HTTP Quit 尚未形成退出汇总，组件已先禁用；本轮按未完成观察处理并清理。");
            }

            ReleaseObservation();
        }

        private sealed class LearningArchitecture : Architecture<LearningArchitecture>
        {
            public LearningArchitecture()
            {
            }

            protected override void Init()
            {
                RegisterSystem<IRuntimeUpdateSystem>(new LearningUpdateSystem());
                RegisterSystem<IHttpRequestSystem>(new HttpRequestSystem());
            }
        }

        private sealed class LearningUpdateSystem : AbstractSystem, IRuntimeUpdateSystem
        {
            private Action listeners;

            public int ListenerCount => listeners?.GetInvocationList().Length ?? 0;

            protected override void OnInit()
            {
            }

            public void RegisterUpdate(Action onUpdate)
            {
                listeners += onUpdate;
            }

            public void UnregisterUpdate(Action onUpdate)
            {
                listeners -= onUpdate;
            }

            public void Tick()
            {
                listeners?.Invoke();
            }
        }
    }
}
