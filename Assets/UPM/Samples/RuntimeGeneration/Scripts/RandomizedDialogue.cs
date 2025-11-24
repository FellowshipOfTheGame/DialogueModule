using System.Collections.Generic;

namespace Fog.Dialogue.Samples.RuntimeGeneration {
    public class RandomizedDialogue : IDialogue {
        // Ideally this value should be dynamic but options is an array and I don't want to deal with this right now =)
        public const int OptionsCount = 2;
        public List<DialogueLine> Lines { get; } = new();
        private readonly IDialogueOption[] options = new IDialogueOption[OptionsCount];
        private readonly DialogueLine question;
        private readonly IDialogueRandomizer randomizer;
        private readonly DialogueEntity speaker;

        public RandomizedDialogue(DialogueEntity speaker, IDialogueRandomizer randomizer) {
            this.speaker = speaker;
            this.randomizer = randomizer;
            question = new RandomizedDialogueLine(speaker, randomizer.Question);
            question.ParseTags(IDialogue.TMProTagFactory);
            for (int index = 0; index < OptionsCount; index++) {
                options[index] = new RandomizingOption(key => {
                    Randomize(key);
                    StartDialogue();
                });
            }
            randomizer.Reset();
            ApplyRandomizedValues();
        }

        private void ApplyRandomizedValues() {
            (question as RandomizedDialogueLine)?.ChangeText(randomizer.Question);
            for (int index = 0; index < OptionsCount; index++) {
                (options[index] as RandomizingOption)?.ChangeValues(randomizer.Options[index], randomizer.Keys[index]);
            }
            for (int index = 0; index < randomizer.Lines.Count; index++) {
                if (index < Lines.Count) {
                    (Lines[index] as RandomizedDialogueLine)?.ChangeText(randomizer.Lines[index]);
                } else {
                    Lines.Add(new RandomizedDialogueLine(speaker, randomizer.Lines[index]));
                    Lines[index].ParseTags(IDialogue.TMProTagFactory);
                }
            }
        }

        private void Randomize(string key) {
            randomizer.Randomize(key);
            ApplyRandomizedValues();
        }

        public void BeforeDialogue() { }

        public void AfterDialogue() {
            DialogueHandler.Instance.DisplayOptions(question, options);
        }

        public void StartDialogue() {
            DialogueHandler.Instance.StartDialogue(this);
        }
    }
}