using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace RPG2D
{
    [RequireComponent(typeof(Button))]
    public class StartSceneButton : RPG2DController
    {
        [SerializeField] private StartSceneButtonType buttonType;
        [SerializeField] private bool requireConfirm = false;
        [SerializeField] private string confirmMessage = "确定执行该操作吗？";

        private Button button;
        private ISceneSystem sceneSystem;

        private void Awake()
        {
            button = GetComponent<Button>();
            sceneSystem = this.GetSystem<ISceneSystem>();

            button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            if(button != null)
            {
                button.onClick.RemoveListener(OnClick);
            }
        }

        private void OnClick()
        {
            if (sceneSystem.IsLoading)
            {
                return;
            }

            if (requireConfirm)
            {
                this.SendCommand(new ShowConfirmPopupCommand(confirmMessage, ExecuteAction));
                return;
            }

            ExecuteAction();
        }

        private void ExecuteAction()
        {
            if (sceneSystem.IsLoading)
            {
                return;
            }

            switch (buttonType)
            {
                case StartSceneButtonType.StartGame:
                    this.SendCommand<StartNewGameCommand>();
                    break;

                case StartSceneButtonType.QuitGame:
                    this.SendCommand<QuitGameCommand>();
                    break;

                case StartSceneButtonType.ReturnToStartScene:
                    this.SendCommand<LoadStartSceneCommand>();
                    break;
            }
        }

    }

    public enum StartSceneButtonType
    {
        StartGame,
        QuitGame,
        ReturnToStartScene,
    }
}

