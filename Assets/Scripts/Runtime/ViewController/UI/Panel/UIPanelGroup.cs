using UnityEngine;

namespace RPG2D
{
    [Tooltip("打开一个面板时，若选择关闭其他面板，则关闭同 Group 的其他面板")]
    public enum UIPanelGroup
    {
        None = 0,
        MainPanel = 1,
        Popup = 2,
        Loading = 3,
        Dialogue = 4,
    }
}
