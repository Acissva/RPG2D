namespace RPG2D
{
    public struct UIPanelOpenedEvent
    {
        public UIPanelId PanelId;
        public UIPanelGroup PanelGroup;
        public UILayer Layer;

        public UIPanelId PreviousTopInputPanelId;
        public UIPanelId CurrentTopInputPanelId;
        public bool IsTopInputPanelChanged;
    }
}
