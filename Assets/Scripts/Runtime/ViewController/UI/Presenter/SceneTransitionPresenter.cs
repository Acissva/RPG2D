using QFramework;
using System;
using UnityEngine;

namespace RPG2D
{
    [DisallowMultipleComponent]
    public sealed class SceneTransitionPresenter : RPG2DController
    {
        private IUISystem uiSystem;
        private Guid activeOperationId;
        private bool initialized;

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            uiSystem = this.GetSystem<IUISystem>();

            this.RegisterEvent<SceneLoadStartedEvent>(OnSceneLoadStarted)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<SceneLoadCompletedEvent>(OnSceneLoadCompleted)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            this.RegisterEvent<SceneLoadFailedEvent>(OnSceneLoadFailed)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            initialized = true;
        }

        private void OnSceneLoadStarted(SceneLoadStartedEvent sceneLoadStartedEvent)
        {
            activeOperationId = sceneLoadStartedEvent.OperationId;

            uiSystem.CloseAllPanels();
            uiSystem.CleanupInvalidUIReferences();
            uiSystem.OpenPanel(UIPanelId.Loading, false);
        }

        private void OnSceneLoadCompleted(SceneLoadCompletedEvent sceneLoadCompletedEvent)
        {
            CloseLoading(sceneLoadCompletedEvent.OperationId);
        }

        private void OnSceneLoadFailed(SceneLoadFailedEvent sceneLoadFailedEvent)
        {
            CloseLoading(sceneLoadFailedEvent.OperationId);
        }

        private void CloseLoading(Guid operationId)
        {
            if (operationId == Guid.Empty || operationId != activeOperationId)
            {
                return;
            }

            activeOperationId = Guid.Empty;
            uiSystem.ClosePanel(UIPanelId.Loading);
            uiSystem.CleanupInvalidUIReferences();
        }
    }
}
