using QFramework;
using TMPro;
using UnityEngine;

namespace RPG2D
{
    public class LoadingPanelController : UIPanelController
    {
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private string defaultText = "加载中...";

        private ISceneSystem sceneSystem;

        private void Awake()
        {
            sceneSystem = this.GetSystem<ISceneSystem>();

            sceneSystem.LoadingProgress.RegisterWithInitValue(RefreshProgress)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        protected override void OnOpen()
        {
            RefreshProgress(sceneSystem.LoadingProgress.Value);
        }

        private void RefreshProgress(float progress)
        {
            if (loadingText == null)
            {
                return;
            }

            int percent = Mathf.RoundToInt(progress * 100f);
            loadingText.text = $"{defaultText} {percent}%";
        }
    }
}
