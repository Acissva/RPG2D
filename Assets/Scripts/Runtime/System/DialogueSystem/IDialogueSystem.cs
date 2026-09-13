using QFramework;
using System.Collections.Generic;

namespace RPG2D
{
    public interface IDialogueSystem : ISystem
    {
        bool IsPlaying { get; }

        void StartDialogue(IReadOnlyList<DialogueLine> dialogueLines);
        void Advance();
        void EndDialogue();
    }
}
