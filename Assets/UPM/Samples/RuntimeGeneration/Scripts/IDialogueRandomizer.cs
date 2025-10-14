using System.Collections.ObjectModel;

namespace Fog.Dialogue.Samples.RuntimeGeneration {
    public interface IDialogueRandomizer {
        public ReadOnlyCollection<string> Options { get; }
        public ReadOnlyCollection<string> Keys { get; }
        public string Question { get; }
        public ReadOnlyCollection<string> Lines { get; }

        public void Reset();

        public void Randomize(string newKey);
    }
}