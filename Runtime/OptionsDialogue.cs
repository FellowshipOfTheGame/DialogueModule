using UnityEngine;

namespace Fog.Dialogue {
    /// <summary>
    ///     Creates a scriptable object for an array of dialogue lines, so that it can be saved as a file.
    /// </summary>
    [CreateAssetMenu(fileName = "NewOptionsDialogue", menuName = "FoG/DialogueModule/OptionsDialogue")]
    public class OptionsDialogue : Dialogue {
        [SerializeField] protected DialogueLine question;
        [SerializeField] protected DialogueOptionInfo[] options;

        protected void CopyFrom(OptionsDialogue otherDialogue) {
            base.CopyFrom(otherDialogue);
            question = (DialogueLine)otherDialogue.question.Clone();
            options = (DialogueOptionInfo[])otherDialogue.options.Clone();
        }

        public override object Clone() {
            OptionsDialogue clone = CreateInstance<OptionsDialogue>();
            clone.CopyFrom(this);
            return clone;
        }

        public override void AfterDialogue() {
            base.AfterDialogue();
            Agent.Instance.BlockInteractions();
            DialogueHandler.instance.DisplayOptions(question, options);
            DialogueHandler.instance.OnDialogueEnd -= AfterDialogue;
        }
    }
}