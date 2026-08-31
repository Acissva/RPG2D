using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace RPG2D
{
    [RequireComponent(typeof(Button))]
    public class UIEntryButton : RPG2DController
    {
        [SerializeField] private UIPanelId targetPanelId;
        [SerializeField] private bool closeOtherPanels = true;

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
            if(targetPanelId == UIPanelId.None)
            {
                Debug.LogWarning($"{nameof(UIEntryButton)} 没有配置 targetPanelId");
                return;
            }

            this.SendCommand(new ToggleUIPanelCommand(targetPanelId, closeOtherPanels));
        }

    }
}

