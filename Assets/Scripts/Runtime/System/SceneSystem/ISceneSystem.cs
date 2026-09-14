using QFramework;
using System;

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
}
