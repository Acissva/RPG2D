using UnityEngine;

namespace RPG2D
{
    public class UIPanelController : RPG2DController
    {
        [SerializeField] private UIPanelConfig config;

        public UIPanelId PanelId => config != null ? config.PanelId : UIPanelId.None;
        public UIPanelGroup PanelGroup => config != null ? config.PanelGroup : UIPanelGroup.None;

        public UILayer Layer => config != null ? config.Layer : UILayer.None;

        public bool CloseOnBack => config != null && config.CloseOnBack;
        public bool BlockInput => config != null && config.BlockInput;

        public int SortOrder => config != null ? config.SortOrder : 0;
        public string PrefabPath => config != null ? config.PrefabPath : string.Empty;
        public UIPanelCacheMode CacheMode => config != null ? config.CacheMode : UIPanelCacheMode.None;


        public bool IsOpen => gameObject.activeSelf;

        public virtual void Open()
        {
            if (IsOpen) return;

            gameObject.SetActive(true);
            OnOpen();
        }

        public virtual void Close()
        {
            if(!IsOpen) return;

            gameObject.SetActive(false);
            OnClose();
        }

        protected virtual void OnOpen()
        {

        }

        protected virtual void OnClose()
        {

        }

        public bool ValidateConfig()
        {
            if(config == null)
            {
                Debug.LogError($"{nameof(UIPanelController)} 缺少 UIPanelConfig", this);
                return false;
            }

            if (PanelId == UIPanelId.None)
            {
                Debug.LogError($"{nameof(UIPanelController)} 的 PanelId 不能为 None", this);
                return false;
            }

            return true;
        }

    }
}
