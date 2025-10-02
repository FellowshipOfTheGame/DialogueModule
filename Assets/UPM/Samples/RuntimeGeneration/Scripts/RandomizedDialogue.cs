using System.Collections.Generic;
using UnityEngine;

namespace Fog.Dialogue.Samples.RuntimeGeneration {
    public class RandomizedDialogue : IDialogue {
        private const int optionsCount = 2;
        private const int lineCount = 4;
        private const string questionPrompt = "question";
        private const string firstKey = "start";
        public List<DialogueLine> Lines { get; } = new();
        private readonly IDialogueOption[] options = new IDialogueOption[optionsCount];
        private readonly DialogueLine question;
        private readonly DialogueEntity speaker;

        public RandomizedDialogue(DialogueEntity speaker) {
            this.speaker = speaker;
            question = new DialogueLine(speaker, questionPrompt);
            question.ParseTags(IDialogue.TMProTagFactory);
            for (int index = 0; index < optionsCount; index++) {
                options[index] = new RandomizingOption(key => {
                    Debug.Log($"Selected key {key}");
                    Randomize(key);
                    StartDialogue();
                });
                (options[index] as RandomizingOption)?.Randomize($"{index + 1}", $"{index + 1}");
            }
            for (int index = 0; index < lineCount; index++) {
                Lines.Add(new RandomizedDialogueLine(speaker, $"dialogue line {index + 1}"));
                Lines[index].ParseTags(IDialogue.TMProTagFactory);
            }
            Randomize(firstKey);
        }

        private void Randomize(string key) { }

        public void BeforeDialogue() { }

        public void AfterDialogue() {
            DialogueHandler.instance.DisplayOptions(question, options);
        }

        public void StartDialogue() {
            DialogueHandler.instance.StartDialogue(this);
        }
    }
}