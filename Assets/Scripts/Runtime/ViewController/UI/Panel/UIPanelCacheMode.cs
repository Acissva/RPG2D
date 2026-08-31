namespace RPG2D
{
    public enum UIPanelCacheMode
    {
        None = 0,

        /// <summary>
        /// 场景已有面板，不由UISystem创建和销毁（场景里常驻的面板）
        /// </summary>
        SceneObject = 1,

        /// <summary>
        /// 关闭后保留实例，后续再次打开复用（动态加载但常用的面板，比如背包、地图）
        /// </summary>
        CacheOnClose = 2,

        /// <summary>
        /// 关闭后销毁实例，后续再次打开重新加载（动态加载且不常用的一次性弹窗）
        /// </summary>
        DestroyOnClose = 3,
    }
}

