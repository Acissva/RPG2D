using System.Collections.Generic;

namespace RPG2D
{
    public class UIStatusSnapshot
    {
        public UIPanelId TopInputPanelId;
        public bool InputBlocked;

        public int RootCount;
        public int RegisteredPanelCount;
        public int OpenedPanelCount;

        public List<UIPanelId> RegisteredPanelIds = new();
        public List<UIPanelId> OpenedPanelIds = new();

        public List<UIPanelStatusInfo> RegisteredPanelInfos = new();
        public List<UIPanelStatusInfo> OpenedPanelInfos = new();

        public List<UIAutoRepairInfo> AutoRepairInfos = new();
    }

    public class UIPanelStatusInfo
    {
        public UIPanelId PanelId;
        public UIPanelGroup PanelGroup;
        public UILayer Layer;
        public UIPanelCacheMode CacheMode;

        public bool IsOpen;
        public bool IsGameObjectActive;
        public bool IsTopInputPanel;
        public bool CloseOnBack;
        public bool BlockInput;

        public bool HasStateMismatch;
        public string StateMismatchReason;

        public int SortOrder;
        public string GameObjectName;
        public string ParentName;

    }

    public class UIAutoRepairInfo
    {
        public UIPanelId PanelId;
        public string Reason;
        public string RepairAction;

        public bool WasSystemOpen;
        public bool WasGameObjectActive;

        public string GameObjectName;
        public string ParentName;
    }

}
