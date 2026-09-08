using QFramework;

namespace RPG2D
{
    public class StartNewGameCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            IPlayerSystem playerSystem = this.GetSystem<IPlayerSystem>();

            if (!playerSystem.InitializeNewGame())
            {
                return;
            }

            this.GetSystem<IInventorySystem>().ResetForNewGame();
            this.GetSystem<IQuestSystem>().ResetForNewGame();
            this.GetSystem<IDungeonSystem>().ResetForNewGame();
            this.GetSystem<ICombatStateSystem>().ClearCombatState();
            this.GetSystem<IDialogueSystem>().EndDialogue();

            this.GetSystem<ISceneSystem>().LoadMainGameScene();
        }
    }
}
