using QFramework;
using System;

namespace RPG2D
{
    public class ShowConfirmPopupCommand : AbstractCommand
    {
        private readonly string message;
        private readonly Action onConfirm;

        public ShowConfirmPopupCommand(string message, Action onConfirm)
        {
            this.message = message;
            this.onConfirm = onConfirm;
        }

        protected override void OnExecute()
        {
            this.GetSystem<IUISystem>().ShowConfirmPopup(message, onConfirm);
        }
    }
}

