namespace RPG2D
{
    public struct UIPanelClosedEvent
    {
        public UIPanelId PanelId;
        public UIPanelGroup PanelGroup;
        public UILayer Layer;

        public UIPanelId PreviousTopPanelId;
        public UIPanelId CurrentTopPanelId;
        public bool IsTopPanelChanged;
    }
}
