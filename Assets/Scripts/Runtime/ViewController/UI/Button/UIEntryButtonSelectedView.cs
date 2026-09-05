using QFramework;
using UnityEngine;
using UnityEngine.UI;

namespace RPG2D
{
    [RequireComponent(typeof(Image))]
    public class UIEntryButtonSelectedView : RPG2DController
    {
        [SerializeField] private UIPanelId targetPanelId;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color selectedColor = new Color(0.7f, 0.9f, 1f, 1f);

        private Image image;
        private IUIModel uiModel;
        private IUISystem uiSystem;

        private void Awake()
        {
            image = GetComponent<Image>();
            uiModel = this.GetModel<IUIModel>();
            uiSystem = this.GetSystem<IUISystem>();

            uiModel.UIRevision.RegisterWithInitValue(_ => Refresh())
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void Refresh()
        {
            if (image == null) return;

            bool selected = uiSystem.IsPanelOpen(targetPanelId);
            image.color = selected ? selectedColor : normalColor;
        }
    }
}
