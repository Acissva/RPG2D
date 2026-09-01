using QFramework;

namespace RPG2D
{
    public interface IDialogueModel : IModel
    {
        public BindableProperty<bool> IsPlaying { get; }
        public BindableProperty<string> CurrentSpeakerName { get; }
        public BindableProperty<string> CurrentText { get; }
        public BindableProperty<int> CurrentLineIndex { get; }
        public BindableProperty<int> TotalLineCount { get; }
    }

    public class DialogueModel : AbstractModel, IDialogueModel
    {
        public BindableProperty<bool> IsPlaying { get; } = new BindableProperty<bool>(false);

        public BindableProperty<string> CurrentSpeakerName { get; } = new BindableProperty<string>(string.Empty);

        public BindableProperty<string> CurrentText { get; } = new BindableProperty<string>(string.Empty);

        public BindableProperty<int> CurrentLineIndex { get; } = new BindableProperty<int>(-1);

        public BindableProperty<int> TotalLineCount { get; } = new BindableProperty<int>(0);

        protected override void OnInit()
        {

        }
    }
}
