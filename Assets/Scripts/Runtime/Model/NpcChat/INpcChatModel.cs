using QFramework;

namespace RPG2D
{
    public interface INpcChatModel : IModel
    {
        BindableProperty<NpcChatSnapshot> Snapshot { get; }
    }

    public class NpcChatModel : AbstractModel, INpcChatModel
    {
        public BindableProperty<NpcChatSnapshot> Snapshot { get; } = new BindableProperty<NpcChatSnapshot>(NpcChatSnapshot.Empty);

        protected override void OnInit()
        {
        }
    }
}
