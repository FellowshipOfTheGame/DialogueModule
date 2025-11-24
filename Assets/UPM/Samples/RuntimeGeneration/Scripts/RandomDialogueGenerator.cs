using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Fog.Dialogue.Samples.RuntimeGeneration {
    [CreateAssetMenu(fileName = "NewRandomDialogueGenerator",
                     menuName = "FoG/DialogueModule/Sample/RuntimeGeneration/RandomDialogueGenerator")]
    public class RandomDialogueGenerator : ScriptableObject, IDialogueRandomizer {
        // Same as OptionsCount, this value could be dynamic but this is a placeholder generator
        private const int lineCount = 4;
        [SerializeField] private string firstKey = "reset";
        [SerializeField] private string[] keyList;
        private readonly string[] options = new string[RandomizedDialogue.OptionsCount];
        private ReadOnlyCollection<string> readonlyOptions = null;
        public ReadOnlyCollection<string> Options => readonlyOptions ??= new ReadOnlyCollection<string>(options);
        private readonly string[] keys = new string[RandomizedDialogue.OptionsCount];
        private ReadOnlyCollection<string> readonlyKeys = null;
        public ReadOnlyCollection<string> Keys => readonlyKeys ??= new ReadOnlyCollection<string>(keys);
        public string Question { get; private set; } = "Select a new string key now";
        private readonly List<string> lines = new();
        private ReadOnlyCollection<string> readonlyLines = null;
        public ReadOnlyCollection<string> Lines => readonlyLines ??= lines.AsReadOnly();

        private readonly List<string> availableKeys = new();
        private readonly List<string> keyHistory = new();

        public void Reset() {
            keyHistory.Clear();
            availableKeys.Clear();
            availableKeys.AddRange(keyList);
            lines.Clear();
            for (int count = 0; count < lineCount; count++) {
                lines.Add("");
            }
            Randomize(firstKey);
        }

        public void Randomize(string newKey) {
            UpdateLines(newKey);
            if (availableKeys.Count < options.Length) {
                availableKeys.AddRange(keyHistory);
                keyHistory.Clear();
            }
            availableKeys.Remove(newKey);
            keyHistory.Add(newKey);
            for (int count = 0; count < options.Length; count++) {
                int randomIndex = Random.Range(0, availableKeys.Count);
                keyHistory.Add(availableKeys[randomIndex]);
                options[count] = $"\"{availableKeys[randomIndex]}\"";
                keys[count] = availableKeys[randomIndex];
                availableKeys.RemoveAt(randomIndex);
            }
        }

        private void UpdateLines(string newKey) {
            if (newKey == firstKey)
                lines[0] = $"The last selected key was {newKey}";
            else
                lines[0] = $"The last selected key was \"{newKey}\"";
            lines[1] = $"This dialogue is being generated and adapted in runtime";
            lines[2] = $"Being just a sample there is no advanced logic to it, but changing this should be simple";
            lines[3] = $"The history of selected keys can be used to adapt the dialogue and options shown";
        }
    }
}