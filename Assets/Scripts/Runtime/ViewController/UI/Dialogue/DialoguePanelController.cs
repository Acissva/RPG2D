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
        private IUISystem uiSystem;
        private IInputReader inputReader;

        //UISystem 动态实例化面板后会先调用一次 panel.Close()，用于初始化为关闭状态。
        //DialoguePanelController.OnClose() 会把这次“初始化关闭”误认为玩家关闭对话，从而提前 EndDialogue()。
        //该字段用于解决这个问题
        private bool hasOpened;

        private void Awake()
        {
            dialogueModel = this.GetModel<IDialogueModel>();
            dialogueSystem = this.GetSystem<IDialogueSystem>();
            uiSystem = this.GetSystem<IUISystem>();
            inputReader = this.GetSystem<IRPG2DInputSystem>().InputReader;

            dialogueModel.CurrentSpeakerName.RegisterWithInitValue(v => RefreshContent())
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            dialogueModel.CurrentText.RegisterWithInitValue(v => RefreshContent())
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            dialogueModel.CurrentLineIndex.RegisterWithInitValue(v => RefreshHint())
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            dialogueModel.TotalLineCount.RegisterWithInitValue(v => RefreshHint())
                .UnRegisterWhenGameObjectDestroyed(gameObject);
            dialogueModel.IsPlaying.RegisterWithInitValue(OnPlayingChanged)
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
            hasOpened = true;

            RefreshContent();
            RefreshHint();
        }

        protected override void OnClose()
        {
            if (!hasOpened)
            {
                return;
            }

            if (dialogueSystem != null && dialogueSystem.IsPlaying)
            {
                dialogueSystem.EndDialogue();
            }
        }

        private void OnPlayingChanged(bool isPlaying)
        {
            if (!isPlaying && IsOpen && uiSystem != null)
            {
                uiSystem.ClosePanel(PanelId);
            }
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
