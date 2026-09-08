using QFramework;

namespace RPG2D
{
    public interface IPlayerModel : IModel
    {
        public BindableProperty<int> HP { get; }
        public BindableProperty<int> MaxHP { get; }
        public BindableProperty<float> MoveSpeed { get; }
        public BindableProperty<PlayerLifeState> LifeState { get; }
    }

    public class PlayerModel : AbstractModel, IPlayerModel
    {
        public BindableProperty<int> HP { get; } = new BindableProperty<int>(5);
        public BindableProperty<int> MaxHP { get; } = new BindableProperty<int>(5);
        public BindableProperty<float> MoveSpeed { get; } = new BindableProperty<float>(5f);
        public BindableProperty<PlayerLifeState> LifeState { get; } =
            new BindableProperty<PlayerLifeState>(PlayerLifeState.Alive);

        protected override void OnInit()
        {

        }
    }
}
