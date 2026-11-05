using QFramework;

namespace RPG2D
{
    public interface INoticeModel : IModel
    {
        BindableProperty<NoticeSnapshot> Snapshot { get; }
    }

    public class NoticeModel : AbstractModel, INoticeModel
    {
        public BindableProperty<NoticeSnapshot> Snapshot { get; } = new BindableProperty<NoticeSnapshot>(NoticeSnapshot.Empty);

        protected override void OnInit()
        {

        }
    }
}
