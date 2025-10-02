using System.Collections.ObjectModel;

namespace Fog.Dialogue.Samples.RuntimeGeneration {
    public class RandomizedDialogueLine : DialogueLine {
        private ReadOnlyDictionary<string, DialogueTextTag.Constructor> currentTagFactory = null;

        public RandomizedDialogueLine(DialogueLine otherLine) : base(otherLine) { }

        public RandomizedDialogueLine(DialogueEntity speaker, string text) : base(speaker, text) { }

        public override void ParseTags(ReadOnlyDictionary<string, DialogueTextTag.Constructor> tagFactory) {
            base.ParseTags(tagFactory);
            currentTagFactory = tagFactory;
        }

        public void ChangeText(string newText) {
            text = newText;
            VisibleString = newText;
            InvisibleString = newText;
            if (currentTagFactory != null) ParseTags(currentTagFactory);
        }
    }
}