using UnityEngine;

namespace Fog.Dialogue {
    /// <summary>
    ///     Creates a scriptable object for an array of dialogue lines, so that it can be saved as a file.
    /// </summary>
    [CreateAssetMenu(fileName = "NewOptionsDialogue", menuName = "FoG/DialogueModule/OptionsDialogue")]
    public class OptionsDialogue : Dialogue {
        [SerializeField] private DialogueLine question;
        [SerializeField] private DialogueOptionInfo[] options;

        public override void AfterDialogue() {
            base.AfterDialogue();
            Agent.Instance.BlockInteractions();
            DialogueHandler.instance.DisplayOptions(question, options);
            DialogueHandler.instance.OnDialogueEnd -= AfterDialogue;
        }
    }
}