using QFramework;
using TMPro;
using UnityEngine;

namespace RPG2D
{
    public class DialoguePanelController : UIPanelController
    {
        [SerializeField] private TextMeshProUGUI speakerNameText;
        [SerializeField] private TextMeshProUGUI bodyText;
        [SerializeField] private TextMeshProUGUI hintText;

        [SerializeField] private string continueHintText = "按 F 继续";
        [SerializeField] private string finishHintText = "按 F 关闭";

        private IDialogueModel dialogueModel;
        private IDialogueSystem dialogueSystem;
        private IInputReader inputReader;

        private void Awake()
        {
            dialogueModel = this.GetModel<IDialogueModel>();
            dialogueSystem = this.GetSystem<IDialogueSystem>();
            inputReader = this.GetSystem<IRPG2DInputSystem>().InputReader;

            dialogueModel.CurrentSpeakerName.RegisterWithInitValue(v => RefreshContent())
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            dialogueModel.CurrentText.RegisterWithInitValue(v => RefreshContent())
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            dialogueModel.CurrentLineIndex.RegisterWithInitValue(v => RefreshHint())
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            dialogueModel.TotalLineCount.RegisterWithInitValue(v => RefreshHint())
                .UnRegisterWhenGameObjectDestroyed(gameObject);
        }

        private void Update()
        {
            if (!IsOpen || inputReader == null)
            {
                return;
            }

            if (inputReader.ConsumeAction(InputActionType.Interact))
            {
                dialogueSystem.Advance();
            }
        }

        protected override void OnOpen()
        {
            RefreshContent();
            RefreshHint();
        }

        private void RefreshContent()
        {
            if (dialogueModel == null)
            {
                return;
            }

            if (speakerNameText != null)
            {
                speakerNameText.text = dialogueModel.CurrentSpeakerName.Value;
            }

            if (bodyText != null)
            {
                bodyText.text = dialogueModel.CurrentText.Value;
            }
        }

        private void RefreshHint()
        {
            if (dialogueModel == null || hintText == null)
            {
                return;
            }

            bool isLastLine = dialogueModel.TotalLineCount.Value > 0
                && dialogueModel.CurrentLineIndex.Value >= dialogueModel.TotalLineCount.Value - 1;

            hintText.text = isLastLine ? finishHintText : continueHintText;
        }
    }
}
