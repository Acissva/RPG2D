using QFramework;
using UnityEngine;

namespace RPG2D
{
    public class SceneDebugLogger : RPG2DController
    {
        private static SceneDebugLogger instance;

        [SerializeField] private bool logSceneChanged = true;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;

            DontDestroyOnLoad(gameObject);

            Init();
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        private void Init()
        {
            this.RegisterEvent<SceneLoadStartedEvent>(OnSceneLoadStarted)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<SceneLoadCompletedEvent>(OnSceneLoadCompleted)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            this.RegisterEvent<SceneLoadFailedEvent>(OnSceneLoadFailed)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void OnSceneLoadStarted(SceneLoadStartedEvent e)
        {
            if (!logSceneChanged) return;

            Debug.Log($"开始切换场景：{e.FromSceneName} -> {e.ToSceneName}");
        }

        private void OnSceneLoadCompleted(SceneLoadCompletedEvent e)
        {
            if(!logSceneChanged) return;

            Debug.Log($"场景切换完成：{e.SceneName}");
        }

        private void OnSceneLoadFailed(SceneLoadFailedEvent e)
        {
            if (!logSceneChanged) return;

            Debug.LogError($"场景切换失败：{e.FromSceneName} -> {e.ToSceneName}，原因：{e.Reason}");
        }
    }
}
