using QFramework;

namespace RPG2D
{
    public class RPG2DInputSystem : AbstractSystem, IRPG2DInputSystem
    {
        public IInputReader InputReader { get; } = new OldInputReader();
        private IRuntimeUpdateSystem runtimeUpdateSystem;

        protected override void OnInit()
        {
            runtimeUpdateSystem = this.GetSystem<IRuntimeUpdateSystem>();
            runtimeUpdateSystem.RegisterUpdate(InputReader.OnUpdate);
        }
    }
}
