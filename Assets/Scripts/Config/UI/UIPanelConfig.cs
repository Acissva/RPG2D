using UnityEngine;

namespace RPG2D
{
    [CreateAssetMenu(menuName = "RPG2D/Config/UI/Panel Config", fileName = "UIPanelConfig")]
    public class UIPanelConfig : ScriptableObject
    {
        [SerializeField] private UIPanelId panelId;
        [SerializeField] private UIPanelGroup panelGroup = UIPanelGroup.MainPanel;
        [SerializeField] private UILayer layer = UILayer.MainPanel;
        /// <summary>
        /// 按下通用返回按钮（ESC）时是否可以返回
        /// </summary>
        [SerializeField] private bool closeOnBack = true;
        /// <summary>
        /// 是否阻挡玩家操作输入
        /// </summary>
        [SerializeField] private bool blockInput = true;

        [SerializeField] private int sortOrder = 0;

        [SerializeField] private string prefabAssetKey;

        [SerializeField] private UIPanelCacheMode cacheMode = UIPanelCacheMode.SceneObject;

        public UIPanelId PanelId => panelId;
        public UIPanelGroup PanelGroup => panelGroup;
        public UILayer Layer => layer;
        public bool CloseOnBack => closeOnBack;
        public bool BlockInput => blockInput;
        public int SortOrder => sortOrder;
        public string PrefabAssetKey => prefabAssetKey;
        public UIPanelCacheMode CacheMode => cacheMode;

        private void OnValidate()
        {
            if (panelId == UIPanelId.None)
            {
                Debug.LogError($"{nameof(UIPanelConfig)} {name} 的 PanelId 不能为 None。", this);
            }

            if (cacheMode == UIPanelCacheMode.None)
            {
                Debug.LogError($"{nameof(UIPanelConfig)} {panelId} 的 CacheMode 不能为 None。", this);
            }

            if (cacheMode != UIPanelCacheMode.SceneObject && string.IsNullOrWhiteSpace(prefabAssetKey))
            {
                Debug.LogError($"{nameof(UIPanelConfig)} {panelId} 使用 {cacheMode} 模式时必须配置 PrefabAssetKey。", this);
            }
        }
    }
}

