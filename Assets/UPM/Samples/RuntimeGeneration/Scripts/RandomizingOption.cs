using UnityEngine.Events;

namespace Fog.Dialogue.Samples.RuntimeGeneration {
    public class RandomizingOption : IDialogueOption {
        private readonly UnityEvent<string> onSelect = new();

        public RandomizingOption(UnityAction<string> callback) {
            onSelect.AddListener(callback);
        }

        public string Text { get; private set; } = "";
        private string key = "";

        public void Select() {
            onSelect.Invoke(key);
        }

        public void Randomize(string text, string newKey) {
            Text = text;
            key = newKey;
        }
    }
}