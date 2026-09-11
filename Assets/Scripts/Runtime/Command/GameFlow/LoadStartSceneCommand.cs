using QFramework;

namespace RPG2D
{
    public class LoadStartSceneCommand : AbstractCommand
    {
        protected override void OnExecute()
        {
            this.GetSystem<IGameFlowSystem>().ResetSession();
            this.GetSystem<ISceneSystem>().LoadStartScene();
        }
    }
}
