using UnityEngine;

namespace RPG2D
{
    [CreateAssetMenu(menuName = "RPG2D/GameConfig/Panel Config", fileName = "UIPanelConfig")]
    public class UIPanelConfig : ScriptableObject
    {
        [SerializeField] private UIPanelId panelId;
        [SerializeField] private UIPanelGroup panelGroup = UIPanelGroup.MainPanel;
        [SerializeField] private UILayer layer = UILayer.Panel;
        /// <summary>
        /// 按下通用返回按钮（ESC）时是否可以返回
        /// </summary>
        [SerializeField] private bool closeOnBack = true;
        /// <summary>
        /// 是否阻挡玩家操作输入
        /// </summary>
        [SerializeField] private bool blockInput = true;

        [SerializeField] private int sortOrder = 0;
        [SerializeField] private string prefabPath;
        [SerializeField] private UIPanelCacheMode cacheMode = UIPanelCacheMode.SceneObject;

        public UIPanelId PanelId => panelId;
        public UIPanelGroup PanelGroup => panelGroup;
        public UILayer Layer => layer;
        public bool CloseOnBack => closeOnBack;
        public bool BlockInput => blockInput;
        public int SortOrder => sortOrder;
        public string PrefabPath => prefabPath;
        public UIPanelCacheMode CacheMode => cacheMode;
    }
}

