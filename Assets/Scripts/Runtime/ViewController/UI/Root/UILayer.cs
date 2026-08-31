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
        Panel = 3,
        Dialogue = 4,
        Popup = 5,
        Toast = 6,
        Loading = 7,
    }

}
