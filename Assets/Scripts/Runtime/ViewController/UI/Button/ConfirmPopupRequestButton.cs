using QFramework;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace RPG2D
{
    //这是一个临时学习用按钮脚本，用于测试确认弹窗
    [RequireComponent(typeof(Button))]
    public class ConfirmPopupRequestButton : RPG2DController
    {
        [SerializeField] private ConfirmPopupAction action;
        [SerializeField] private string message = "确定执行该操作吗？";

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
            button.onClick.AddListener(OnClick);
        }

        private void OnDestroy()
        {
            if (button != null)
            {
                button.onClick.RemoveListener(OnClick);
            }
        }

        private void OnClick()
        {
            this.SendCommand(new ShowConfirmPopupCommand(message, ExecuteAction));
        }

        private void ExecuteAction()
        {
            switch (action)
            {
                case ConfirmPopupAction.QuitGame:
                    this.SendCommand<QuitGameCommand>();
                    break;

                case ConfirmPopupAction.LoadStartScene:
                    this.SendCommand<LoadStartSceneCommand>();
                    break;
            }
        }

    }

    public enum ConfirmPopupAction
    {
        QuitGame,
        LoadStartScene
    }
}
