using QFramework;
using System.Collections.Generic;
using UnityEngine;

namespace RPG2D
{
    public interface IDialogueSystem : ISystem
    {
        bool IsPlaying { get; }

        void StartDialogue(IReadOnlyList<DialogueLine> dialogueLines);
        void Advance();
        void EndDialogue();
    }

    public class DialogueSystem : AbstractSystem, IDialogueSystem
    {
        private readonly List<DialogueLine> currentLines = new List<DialogueLine>();

        private IDialogueModel dialogueModel;

        public bool IsPlaying => dialogueModel != null && dialogueModel.IsPlaying.Value;

        protected override void OnInit()
        {
            dialogueModel = this.GetModel<IDialogueModel>();
        }

        public void StartDialogue(IReadOnlyList<DialogueLine> dialogueLines)
        {
            currentLines.Clear();

            if (dialogueLines == null || dialogueLines.Count == 0)
            {
                EndDialogue();
                return;
            }

            for (int i = 0; i < dialogueLines.Count; i++)
            {
                DialogueLine line = dialogueLines[i];

                if (line == null || string.IsNullOrWhiteSpace(line.Text))
                {
                    continue;
                }

                currentLines.Add(line);
            }

            if (currentLines.Count == 0)
            {
                EndDialogue();
                return;
            }

            dialogueModel.TotalLineCount.Value = currentLines.Count;
            ShowLine(0);
            dialogueModel.IsPlaying.Value = true;
        }

        public void Advance()
        {
            if (!IsPlaying)
            {
                return;
            }

            int nextIndex = dialogueModel.CurrentLineIndex.Value + 1;

            if (nextIndex >= currentLines.Count)
            {
                EndDialogue();
                return;
            }

            ShowLine(nextIndex);
        }

        public void EndDialogue()
        {
            bool wasPlaying = IsPlaying;

            currentLines.Clear();

            dialogueModel.CurrentSpeakerName.Value = string.Empty;
            dialogueModel.CurrentText.Value = string.Empty;
            dialogueModel.CurrentLineIndex.Value = -1;
            dialogueModel.TotalLineCount.Value = 0;
            dialogueModel.IsPlaying.Value = false;

            if (wasPlaying)
            {
                Debug.Log("对话结束");
            }
        }

        private void ShowLine(int lineIndex)
        {
            DialogueLine line = currentLines[lineIndex];

            dialogueModel.CurrentLineIndex.Value = lineIndex;
            dialogueModel.CurrentSpeakerName.Value = string.IsNullOrWhiteSpace(line.SpeakerName)
                ? "旁白"
                : line.SpeakerName;
            dialogueModel.CurrentText.Value = line.Text;

            Debug.Log($"对话 {lineIndex + 1}/{currentLines.Count} {dialogueModel.CurrentSpeakerName.Value}：{dialogueModel.CurrentText.Value}");
        }
    }
}
