using QFramework;
using UnityEngine;

namespace RPG2D
{
    [DisallowMultipleComponent]
    public sealed class DialoguePresenter : RPG2DController
    {
        private IDialogueModel dialogueModel;
        private IDialogueSystem dialogueSystem;
        private IUISystem uiSystem;
        private bool initialized;

        public void Initialize()
        {
            if (initialized)
            {
                return;
            }

            dialogueModel = this.GetModel<IDialogueModel>();
            dialogueSystem = this.GetSystem<IDialogueSystem>();
            uiSystem = this.GetSystem<IUISystem>();

            this.RegisterEvent<UIPanelClosedEvent>(OnPanelClosed)
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            dialogueModel.IsPlaying.RegisterWithInitValue(OnPlayingChanged)
                .UnRegisterWhenGameObjectDestroyed(gameObject);

            initialized = true;
        }

        private void OnPlayingChanged(bool isPlaying)
        {
            if (isPlaying)
            {
                uiSystem.OpenPanel(UIPanelId.Dialogue, false);
                return;
            }

            if (uiSystem.IsPanelOpen(UIPanelId.Dialogue))
            {
                uiSystem.ClosePanel(UIPanelId.Dialogue);
            }
        }

        private void OnPanelClosed(UIPanelClosedEvent panelClosedEvent)
        {
            if (panelClosedEvent.PanelId != UIPanelId.Dialogue || !dialogueSystem.IsPlaying)
            {
                return;
            }

            dialogueSystem.EndDialogue();
        }
    }
}
