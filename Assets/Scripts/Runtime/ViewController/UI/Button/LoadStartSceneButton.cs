using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace RPG2D
{
    [RequireComponent(typeof(Button))]
    public class LoadStartSceneButton : RPG2DController
    {
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
            this.SendCommand<LoadStartSceneCommand>();
        }
    }
}
