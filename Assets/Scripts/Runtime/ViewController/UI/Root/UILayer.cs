namespace RPG2D
{
    /// <summary>
    /// UI层级及渲染排序，值越大越渲染在上层
    /// </summary>
    public enum UILayer
    {
        None = 0,
        HUD = 1,
        Toolbar = 2,
        MainPanel = 3,
        Dialogue = 4,
        Popup = 5,
        Toast = 6,
        Loading = 7,
    }
    //注意：MainPanel 和 Panel 不是一个概念
    //MainPanel是指一种Panel类型（主面板类型）
    //UISystem、UIPanelConfig等中的 Panel 则为面板概念的统称
}
