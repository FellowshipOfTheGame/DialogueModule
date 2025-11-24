using UnityEngine;

namespace Fog.Dialogue.Samples.Example {
    [CreateAssetMenu(fileName = "NewDialogue", menuName = "FoG/DialogueModule/Sample/Example/DialogueSample")]
    public class DialogueBlockMovement : Dialogue {
        public override void BeforeDialogue() {
            SimpleMove.instance.BlockMovement();

            base.BeforeDialogue();
        }

        public override void AfterDialogue() {
            SimpleMove.instance.AllowMovement();

            base.AfterDialogue();
        }
    }
}