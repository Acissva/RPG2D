using QFramework;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG2D
{
    public interface ISceneSystem : ISystem
    {
        string CurrentSceneName { get; }
        string LoadingSceneName { get; }
        bool IsLoading { get; }
        BindableProperty<float> LoadingProgress { get; }

        void LoadStartScene();
        void LoadMainGameScene();
        bool LoadScene(string sceneName, Guid operationId);
    }

    public class SceneSystem : AbstractSystem, ISceneSystem
    {
        public const string StartSceneName = "StartScene";
        public const string MainGameSceneName = "MainGameScene";

        private IRuntimeUpdateSystem runtimeUpdateSystem;
        private IUISystem uiSystem;

        private AsyncOperation loadingOperation;
        private Guid loadingOperationId;
        private string loadingSceneName;
        private bool isLoading;
        private BindableProperty<float> loadingProgress = new BindableProperty<float>(0f);

        public string CurrentSceneName => SceneManager.GetActiveScene().name;

        public string LoadingSceneName => loadingSceneName;

        public bool IsLoading => isLoading;

        public BindableProperty<float> LoadingProgress => loadingProgress;


        protected override void OnInit()
        {
            runtimeUpdateSystem = this.GetSystem<IRuntimeUpdateSystem>();
            uiSystem = this.GetSystem<IUISystem>();

            runtimeUpdateSystem.RegisterUpdate(OnUpdate);
        }

        public void LoadStartScene()
        {
            LoadScene(StartSceneName, Guid.NewGuid());
        }

        public void LoadMainGameScene()
        {
            LoadScene(MainGameSceneName, Guid.NewGuid());
        }

        /// <returns>
        /// false：请求在开始前就被拒绝，没有发送 Started <br />
        /// true：请求已经进入 Scene 生命周期并发送 Started，不代表最终加载成功
        /// </returns>
        public bool LoadScene(string sceneName, Guid operationId)
        {
            if (operationId == Guid.Empty)
            {
                Debug.LogError("场景加载失败：OperationId 无效");
                return false;
            }

            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("场景加载失败：sceneName 为空");
                return false;
            }

            if (isLoading)
            {
                Debug.LogWarning($"场景正在加载中，忽略新的加载请求：{sceneName}");
                return false;
            }

            var fromSceneName = CurrentSceneName;

            if (fromSceneName == sceneName)
            {
                Debug.LogWarning($"已经在目标场景中，忽略重复加载：{sceneName}");
                return false;
            }

            loadingProgress.Value = 0f;
            isLoading = true;
            loadingSceneName = sceneName;
            loadingOperationId = operationId;

            this.SendEvent(new SceneLoadStartedEvent
            {
                OperationId = loadingOperationId,
                FromSceneName = fromSceneName,
                ToSceneName = sceneName
            });

            uiSystem?.CloseAllPanels();
            uiSystem?.CleanupInvalidUIReferences();
            uiSystem?.OpenPanel(UIPanelId.Loading, false);

            try
            {
                loadingOperation = SceneManager.LoadSceneAsync(sceneName);
            }
            catch (Exception e)
            {
                FailLoading(fromSceneName, sceneName, e.Message);
                return true;
            }

            if (loadingOperation == null)
            {
                FailLoading(fromSceneName, sceneName, "SceneManager.LoadSceneAsync 返回空，请检查场景是否已加入 Build Settings");
            }

            return true;
        }

        private void OnUpdate()
        {
            if (!isLoading || loadingOperation == null)
            {
                return;
            }

            UpdateLoadingProgress();

            if (!loadingOperation.isDone)
            {
                return;
            }

            FinishLoading();
        }

        private void FinishLoading()
        {
            Guid completedOperationId = loadingOperationId;
            string completedSceneName = loadingSceneName;

            loadingOperationId = Guid.Empty;
            loadingProgress.Value = 1f;
            loadingOperation = null;
            loadingSceneName = string.Empty;
            isLoading = false;

            uiSystem?.ClosePanel(UIPanelId.Loading);
            uiSystem?.CleanupInvalidUIReferences();

            this.SendEvent(new SceneLoadCompletedEvent
            {
                OperationId = completedOperationId,
                SceneName = completedSceneName
            });
        }

        private void UpdateLoadingProgress()
        {
            //Unity 的 AsyncOperation.progress 在场景加载接近完成时，通常会停在 0.9，最后真正切换/激活场景完成后才变成 1
            //这里意思是把 Unity 的 0 ~ 0.9 映射成 UI 上更好理解的 0 ~ 1
            float progress = Mathf.Clamp01(loadingOperation.progress / 0.9f);

            if (Mathf.Approximately(progress, loadingProgress.Value))
            {
                return;
            }

            loadingProgress.Value = progress;
        }

        private void FailLoading(string fromSceneName, string toSceneName, string reason)
        {
            Guid failedOperationId = loadingOperationId;

            loadingOperationId = Guid.Empty;
            isLoading = false;
            loadingProgress.Value = 0f;
            loadingOperation = null;
            loadingSceneName = string.Empty;

            uiSystem?.ClosePanel(UIPanelId.Loading);
            uiSystem?.CleanupInvalidUIReferences();

            Debug.LogError($"场景切换失败：{fromSceneName} -> {toSceneName}，原因：{reason}");

            this.SendEvent(new SceneLoadFailedEvent
            {
                OperationId = failedOperationId,
                FromSceneName = fromSceneName,
                ToSceneName = toSceneName,
                Reason = reason
            });
        }
    }
}
