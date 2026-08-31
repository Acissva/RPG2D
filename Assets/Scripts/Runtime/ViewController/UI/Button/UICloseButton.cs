using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace RPG2D
{
    [RequireComponent(typeof(Button))]
    public class UICloseButton : RPG2DController
    {
        [SerializeField] private UIPanelId targetPanelId;

        private Button button;

        private void Awake()
        {
            button = GetComponent<Button>();
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
            if (targetPanelId == UIPanelId.None)
            {
                Debug.LogWarning($"{nameof(UICloseButton)} 没有配置 targetPanelId");
                return;
            }

            this.SendCommand(new CloseUIPanelCommand(targetPanelId));
        }

    }
}

