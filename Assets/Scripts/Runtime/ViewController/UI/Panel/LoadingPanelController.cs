using QFramework;
using TMPro;
using UnityEngine;

namespace RPG2D
{
    public class LoadingPanelController : UIPanelController
    {
        [SerializeField] private TextMeshProUGUI loadingText;
        [SerializeField] private string defaultText = "加载中...";

        private void Awake()
        {
            this.RegisterEvent<SceneLoadProgressEvent>(OnSceneLoadProgress)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        protected override void OnOpen()
        {
            RefreshProgress(0f);
        }

        private void OnSceneLoadProgress(SceneLoadProgressEvent e)
        {
            RefreshProgress(e.Progress);
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

