using UnityEngine;

namespace RPG2D
{
    public class UIPanelController : RPG2DController
    {
        [SerializeField] private UIPanelId panelId;

        private UIPanelConfig config;

        public UIPanelId PanelId => panelId;
        public bool IsInitialized => config != null;

        public UIPanelGroup PanelGroup => config != null ? config.PanelGroup : UIPanelGroup.None;
        public UILayer Layer => config != null ? config.Layer : UILayer.None;
        public bool CloseOnBack => config != null && config.CloseOnBack;
        public bool BlockInput => config != null && config.BlockInput;
        public int SortOrder => config != null ? config.SortOrder : 0;
        public string PrefabAssetKey => config != null ? config.PrefabAssetKey : string.Empty;
        public UIPanelCacheMode CacheMode => config != null ? config.CacheMode : UIPanelCacheMode.None;

        public bool IsOpen => gameObject.activeSelf;

        public bool Initialize(UIPanelConfig config)
        {
            if (config == null)
            {
                Debug.LogError($"{nameof(UIPanelController)} 初始化失败：UIPanelConfig 为空", this);
                return false;
            }

            if (panelId == UIPanelId.None)
            {
                Debug.LogError($"{nameof(UIPanelController)} 初始化失败：PanelId 不能为 None", this);
                return false;
            }

            if (config.PanelId != panelId)
            {
                Debug.LogError($"{nameof(UIPanelController)} 初始化失败：PanelId={panelId}，配置 PanelId={config.PanelId}，两者不一致", this);
                return false;
            }

            this.config = config;
            return true;
        }

        public virtual void Open()
        {
            if (IsOpen) return;

            gameObject.SetActive(true);
            OnOpen();
        }

        public virtual void Close()
        {
            if (!IsOpen) return;

            gameObject.SetActive(false);
            OnClose();
        }

        protected virtual void OnOpen()
        {

        }

        protected virtual void OnClose()
        {

        }
    }
}
