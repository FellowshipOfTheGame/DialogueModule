using UnityEngine;

namespace Fog.Dialogue {
    /// <summary>
    ///     Creates a scriptable object for an array of dialogue lines, so that it can be saved as a file.
    /// </summary>
    [CreateAssetMenu(fileName = "NewOptionsDialogue", menuName = "FoG/DialogueModule/OptionsDialogue")]
    public class OptionsDialogue : Dialogue {
        [SerializeField] protected DialogueLine question;
        [SerializeField] protected DialogueOptionInfo[] options;
        protected IDialogueOption[] optionsCast = null;
        protected IDialogueOption[] OptionsCast {
            get {
                if (optionsCast != null) return optionsCast;

                optionsCast = new IDialogueOption[options.Length];
                for (int index = 0; index < options.Length; index++) {
                    optionsCast[index] = options[index];
                }
                return optionsCast;
            }
        }

        protected void CopyFrom(OptionsDialogue otherDialogue) {
            base.CopyFrom(otherDialogue);
            question.CopyFrom(otherDialogue.question);
            options = new DialogueOptionInfo[otherDialogue.options.Length];
            otherDialogue.options.CopyTo(options, 0);
        }

        protected override void ParseLineTags() {
            base.ParseLineTags();
            question.ParseTags(IDialogue.TMProTagFactory);
        }

        public override object Clone() {
            OptionsDialogue clone = CreateInstance<OptionsDialogue>();
            clone.CopyFrom(this);
            return clone;
        }

        public override void AfterDialogue() {
            base.AfterDialogue();
            if (Agent.Instance) Agent.Instance.BlockInteractions();
            DialogueHandler.instance.DisplayOptions(question, OptionsCast);
        }
    }
}