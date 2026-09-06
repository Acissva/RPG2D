using QFramework;
using System.Collections.Generic;

namespace RPG2D
{
    public interface IEncounterModel : IModel
    {
        Dictionary<string, EncounterRuntimeState> Encounters { get; }
        BindableProperty<int> EncounterRevision { get; }
    }

    public class EncounterModel : AbstractModel, IEncounterModel
    {
        public Dictionary<string, EncounterRuntimeState> Encounters { get; } =
            new Dictionary<string, EncounterRuntimeState>();

        public BindableProperty<int> EncounterRevision { get; } =
            new BindableProperty<int>(0);

        protected override void OnInit()
        {
        }
    }
}
