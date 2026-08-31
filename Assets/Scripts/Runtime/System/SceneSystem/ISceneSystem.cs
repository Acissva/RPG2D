using QFramework;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG2D
{
    public interface ISceneSystem : ISystem
    {
        string CurrentSceneName { get; }
        string LoadingSceneName { get; }
        bool IsLoading { get; }
        float LoadingProgress { get; }

        void LoadStartScene();
        void LoadMainGameScene();
    }

    public class SceneSystem : AbstractSystem, ISceneSystem
    {
        public const string StartSceneName = "StartScene";
        public const string MainGameSceneName = "MainGameScene";

        private IRuntimeUpdateSystem runtimeUpdateSystem;
        private IUISystem uiSystem;

        private AsyncOperation loadingOperation;
        private string loadingSceneName;
        private bool isLoading;
        private float loadingProgress;

        public string CurrentSceneName => SceneManager.GetActiveScene().name;

        public string LoadingSceneName => loadingSceneName;

        public bool IsLoading => isLoading;

        public float LoadingProgress => loadingProgress;


        protected override void OnInit()
        {
            runtimeUpdateSystem = this.GetSystem<IRuntimeUpdateSystem>();
            uiSystem = this.GetSystem<IUISystem>();

            runtimeUpdateSystem.RegisterUpdate(OnUpdate);
        }

        public void LoadStartScene()
        {
            LoadScene(StartSceneName);
        }

        public void LoadMainGameScene()
        {
            LoadScene(MainGameSceneName);
        }

        private void LoadScene(string sceneName)
        {
            if (string.IsNullOrWhiteSpace(sceneName))
            {
                Debug.LogError("场景加载失败：sceneName 为空");
                return;
            }

            if (isLoading)
            {
                Debug.LogWarning($"场景正在加载中，忽略新的加载请求：{sceneName}");
                return;
            }

            var fromSceneName = CurrentSceneName;

            if (fromSceneName == sceneName)
            {
                Debug.LogWarning($"已经在目标场景中，忽略重复加载：{sceneName}");
                return;
            }

            loadingProgress = 0f;
            isLoading = true;
            loadingSceneName = sceneName;

            this.SendEvent(new SceneLoadStartedEvent
            {
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
            catch (System.Exception e)
            {
                FailLoading(fromSceneName, sceneName, e.Message);
                return;
            }

            if (loadingOperation == null)
            {
                FailLoading(fromSceneName, sceneName, "SceneManager.LoadSceneAsync 返回空，请检查场景是否已加入 Build Settings");
            }
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
            isLoading = false;

            loadingProgress = 1f;

            this.SendEvent(new SceneLoadProgressEvent
            {
                SceneName = loadingSceneName,
                Progress = loadingProgress
            });

            this.SendEvent(new SceneLoadCompletedEvent
            {
                SceneName = loadingSceneName,
            });

            uiSystem?.ClosePanel(UIPanelId.Loading);
            uiSystem?.CleanupInvalidUIReferences();

            loadingOperation = null;
            loadingSceneName = string.Empty;
        }

        private void UpdateLoadingProgress()
        {
            //Unity 的 AsyncOperation.progress 在场景加载接近完成时，通常会停在 0.9，最后真正切换/激活场景完成后才变成 1
            //这里意思是把 Unity 的 0 ~ 0.9 映射成 UI 上更好理解的 0 ~ 1
            float progress = Mathf.Clamp01(loadingOperation.progress / 0.9f);

            if(Mathf.Approximately(progress, loadingProgress))
            {
                return;
            }

            loadingProgress = progress;

            this.SendEvent(new SceneLoadProgressEvent
            {
                SceneName = loadingSceneName,
                Progress = loadingProgress,
            });
        }

        private void FailLoading(string fromSceneName, string toSceneName, string reason)
        {
            isLoading = false;
            loadingProgress = 0f;
            loadingOperation = null;
            loadingSceneName = string.Empty;

            uiSystem?.ClosePanel(UIPanelId.Loading);
            uiSystem?.CleanupInvalidUIReferences();

            Debug.LogError($"场景切换失败：{fromSceneName} -> {toSceneName}，原因：{reason}");

            this.SendEvent(new SceneLoadFailedEvent
            {
                FromSceneName = fromSceneName,
                ToSceneName = toSceneName,
                Reason = reason
            });
        }
    }
}

