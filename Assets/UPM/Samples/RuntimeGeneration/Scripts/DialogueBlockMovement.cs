using UnityEngine;

namespace Fog.Dialogue.Samples.RuntimeGeneration {
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "FoG/DialogueModule/Sample/RuntimeGeneration/DialogueSample")]
    public class DialogueBlockMovement : Dialogue {
        public override void BeforeDialogue() {
            SimpleMove.instance.BlockMovement();

            base.BeforeDialogue();
            DialogueHandler.instance.OnDialogueStart -= BeforeDialogue;
        }

        public override void AfterDialogue() {
            SimpleMove.instance.AllowMovement();

            base.AfterDialogue();
            DialogueHandler.instance.OnDialogueStart -= AfterDialogue;
        }
    }
}