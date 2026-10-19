using QFramework;

namespace RPG2D
{
    public interface IGameServerModel : IModel
    {
        BindableProperty<GameServerSnapshot> Snapshot { get; }
    }

    public class GameServerModel : AbstractModel, IGameServerModel
    {
        public BindableProperty<GameServerSnapshot> Snapshot { get; } = new BindableProperty<GameServerSnapshot>(GameServerSnapshot.Empty);

        protected override void OnInit()
        {

        }
    }
}
